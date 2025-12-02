using System;
using System.Collections.Generic;
using System.Linq;

namespace PseudoRun.Desktop.Interpreter
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            // Filter out comments
            _tokens = tokens.Where(t => t.Type != TokenType.COMMENT).ToList();
        }

        public List<IASTNode> Parse()
        {
            var statements = new List<IASTNode>();

            while (!IsAtEnd())
            {
                SkipNewlines();
                if (IsAtEnd()) break;

                // Special handling for DECLARE to support comma-separated identifiers
                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "DECLARE")
                {
                    var declareNodes = ParseDeclareStatements();
                    statements.AddRange(declareNodes);
                }
                else
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                    {
                        statements.Add(stmt);
                    }
                }
                SkipNewlines();
            }

            return statements;
        }

        private List<DeclareNode> ParseDeclareStatements()
        {
            int line = Advance().Line; // consume DECLARE
            var nodes = new List<DeclareNode>();

            // Collect all identifiers (comma-separated)
            var identifiers = new List<string>();
            identifiers.Add(Consume(TokenType.IDENTIFIER, "Expected identifier after DECLARE").Value);

            while (Check(TokenType.COMMA))
            {
                Advance(); // consume comma
                identifiers.Add(Consume(TokenType.IDENTIFIER, "Expected identifier after comma").Value);
            }

            Consume(TokenType.COLON, "Expected : after identifier(s) in DECLARE");

            // Check for ARRAY
            if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "ARRAY")
            {
                Advance(); // consume ARRAY

                // Parse bounds
                Consume(TokenType.LBRACKET, "Expected [ after ARRAY");
                var dimensions = new List<ArrayDimension>();

                while (true)
                {
                    var lowerExpr = ParseExpression();
                    if (lowerExpr.Type != "Literal")
                    {
                        throw new Exception($"Array bounds must be literal numbers at line {line}");
                    }
                    int lower = Convert.ToInt32(((LiteralNode)lowerExpr).Value);

                    Consume(TokenType.COLON, "Expected : in array bounds");

                    var upperExpr = ParseExpression();
                    if (upperExpr.Type != "Literal")
                    {
                        throw new Exception($"Array bounds must be literal numbers at line {line}");
                    }
                    int upper = Convert.ToInt32(((LiteralNode)upperExpr).Value);

                    dimensions.Add(new ArrayDimension(lower, upper));

                    if (Check(TokenType.COMMA))
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(TokenType.RBRACKET, "Expected ] after array bounds");
                Consume(TokenType.KEYWORD, "Expected OF after array bounds");
                if (Previous().Value.ToUpper() != "OF")
                {
                    throw new Exception($"Expected OF after array bounds at line {line}");
                }

                DataType elementType = ParseDataType();

                // Create a DeclareNode for each identifier with array type
                foreach (var identifier in identifiers)
                {
                    nodes.Add(new DeclareNode
                    {
                        Line = line,
                        Identifier = identifier,
                        DataType = DataType.ARRAY,
                        ArrayBounds = new ArrayBounds { Dimensions = dimensions },
                        ArrayElementType = elementType
                    });
                }
            }
            else
            {
                // Regular variable
                DataType dataType = ParseDataType();

                // Create a DeclareNode for each identifier
                foreach (var identifier in identifiers)
                {
                    nodes.Add(new DeclareNode
                    {
                        Line = line,
                        Identifier = identifier,
                        DataType = dataType
                    });
                }
            }

            return nodes;
        }

        private IASTNode? ParseStatement()
        {
            Token token = Peek();

            if (token.Type == TokenType.KEYWORD)
            {
                string keyword = token.Value.ToUpper();
                switch (keyword)
                {
                    case "OUTPUT":
                        return ParseOutput();
                    case "INPUT":
                        return ParseInput();
                    case "IF":
                        return ParseIf();
                    case "WHILE":
                        return ParseWhile();
                    case "REPEAT":
                        return ParseRepeat();
                    case "FOR":
                        return ParseFor();
                    case "CASE":
                        return ParseCase();
                    case "PROCEDURE":
                        return ParseProcedure();
                    case "FUNCTION":
                        return ParseFunction();
                    case "CALL":
                        return ParseCall();
                    case "RETURN":
                        return ParseReturn();
                    case "OPENFILE":
                        return ParseOpenFile();
                    case "CLOSEFILE":
                        return ParseCloseFile();
                    case "READFILE":
                        return ParseReadFile();
                    case "WRITEFILE":
                        return ParseWriteFile();
                    case "CONSTANT":
                        throw new Exception($"Feature not supported: {token.Value} at line {token.Line}");
                    default:
                        throw new Exception($"Unexpected keyword '{token.Value}' at line {token.Line}");
                }
            }

            // Check for assignment
            if (token.Type == TokenType.IDENTIFIER)
            {
                return ParseAssignment();
            }

            if (token.Type == TokenType.NEWLINE)
            {
                return null;
            }

            throw new Exception($"Unexpected token '{token.Value}' at line {token.Line}");
        }

        private AssignmentNode ParseAssignment()
        {
            int line = Peek().Line;
            string identifier = Advance().Value;

            IExpressionNode target;

            if (Check(TokenType.LBRACKET))
            {
                // Array access
                Advance(); // consume [
                var indices = new List<IExpressionNode>();

                while (true)
                {
                    indices.Add(ParseExpression());
                    if (Check(TokenType.COMMA))
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(TokenType.RBRACKET, "Expected ] after array indices");

                target = new ArrayAccessNode
                {
                    Line = line,
                    Array = identifier,
                    Indices = indices
                };
            }
            else
            {
                target = new IdentifierNode
                {
                    Line = line,
                    Name = identifier
                };
            }

            Consume(TokenType.ASSIGNMENT, "Expected <- in assignment");
            IExpressionNode value = ParseExpression();

            return new AssignmentNode
            {
                Line = line,
                Target = target,
                Value = value
            };
        }

        private OutputNode ParseOutput()
        {
            int line = Advance().Line; // consume OUTPUT

            var expressions = new List<IExpressionNode>();

            while (true)
            {
                expressions.Add(ParseExpression());
                if (Check(TokenType.COMMA))
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            return new OutputNode
            {
                Line = line,
                Expressions = expressions
            };
        }

        private InputNode ParseInput()
        {
            int line = Advance().Line; // consume INPUT

            string identifier = Consume(TokenType.IDENTIFIER, "Expected identifier after INPUT").Value;

            IExpressionNode target;

            if (Check(TokenType.LBRACKET))
            {
                // Array access
                Advance(); // consume [
                var indices = new List<IExpressionNode>();

                while (true)
                {
                    indices.Add(ParseExpression());
                    if (Check(TokenType.COMMA))
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(TokenType.RBRACKET, "Expected ] after array indices");

                target = new ArrayAccessNode
                {
                    Line = line,
                    Array = identifier,
                    Indices = indices
                };
            }
            else
            {
                target = new IdentifierNode
                {
                    Line = line,
                    Name = identifier
                };
            }

            return new InputNode
            {
                Line = line,
                Target = target
            };
        }

        private IfNode ParseIf()
        {
            int line = Advance().Line; // consume IF

            IExpressionNode condition = ParseExpression();
            Consume(TokenType.KEYWORD, "Expected THEN after IF condition");
            if (Previous().Value.ToUpper() != "THEN")
            {
                throw new Exception($"Expected THEN after IF condition at line {line}");
            }

            SkipNewlines();
            List<IASTNode> thenBlock = ParseBlock(new[] { "ELSE", "ENDIF" });

            var elseIfBlocks = new List<ElseIfBlock>();
            List<IASTNode>? elseBlock = null;

            while (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "ELSE")
            {
                Advance(); // consume ELSE

                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "IF")
                {
                    Advance(); // consume IF

                    IExpressionNode elseIfCondition = ParseExpression();
                    Consume(TokenType.KEYWORD, "Expected THEN after ELSE IF condition");
                    if (Previous().Value.ToUpper() != "THEN")
                    {
                        throw new Exception($"Expected THEN after ELSE IF condition at line {Previous().Line}");
                    }

                    SkipNewlines();
                    List<IASTNode> elseIfBlock = ParseBlock(new[] { "ELSE", "ENDIF" });
                    elseIfBlocks.Add(new ElseIfBlock
                    {
                        Condition = elseIfCondition,
                        Block = elseIfBlock
                    });
                }
                else
                {
                    SkipNewlines();
                    elseBlock = ParseBlock(new[] { "ENDIF" });
                    break;
                }
            }

            Consume(TokenType.KEYWORD, "Expected ENDIF to close IF statement");
            if (Previous().Value.ToUpper() != "ENDIF")
            {
                throw new Exception($"Expected ENDIF to close IF statement at line {line}");
            }

            return new IfNode
            {
                Line = line,
                Condition = condition,
                ThenBlock = thenBlock,
                ElseIfBlocks = elseIfBlocks.Count > 0 ? elseIfBlocks : null,
                ElseBlock = elseBlock
            };
        }

        private WhileNode ParseWhile()
        {
            int line = Advance().Line; // consume WHILE

            IExpressionNode condition = ParseExpression();
            Consume(TokenType.KEYWORD, "Expected DO after WHILE condition");
            if (Previous().Value.ToUpper() != "DO")
            {
                throw new Exception($"Expected DO after WHILE condition at line {line}");
            }

            SkipNewlines();
            List<IASTNode> body = ParseBlock(new[] { "ENDWHILE" });

            Consume(TokenType.KEYWORD, "Expected ENDWHILE to close WHILE loop");
            if (Previous().Value.ToUpper() != "ENDWHILE")
            {
                throw new Exception($"Expected ENDWHILE to close WHILE loop at line {line}");
            }

            return new WhileNode
            {
                Line = line,
                Condition = condition,
                Body = body
            };
        }

        private RepeatNode ParseRepeat()
        {
            int line = Advance().Line; // consume REPEAT

            SkipNewlines();
            List<IASTNode> body = ParseBlock(new[] { "UNTIL" });

            Consume(TokenType.KEYWORD, "Expected UNTIL after REPEAT block");
            if (Previous().Value.ToUpper() != "UNTIL")
            {
                throw new Exception($"Expected UNTIL after REPEAT block at line {line}");
            }

            IExpressionNode condition = ParseExpression();

            return new RepeatNode
            {
                Line = line,
                Body = body,
                Condition = condition
            };
        }

        private ForNode ParseFor()
        {
            int line = Advance().Line; // consume FOR

            string variable = Consume(TokenType.IDENTIFIER, "Expected variable name after FOR").Value;
            Consume(TokenType.ASSIGNMENT, "Expected <- after FOR variable");

            IExpressionNode start = ParseExpression();

            Consume(TokenType.KEYWORD, "Expected TO in FOR loop");
            if (Previous().Value.ToUpper() != "TO")
            {
                throw new Exception($"Expected TO in FOR loop at line {line}");
            }

            IExpressionNode end = ParseExpression();

            // Check for STEP
            IExpressionNode step = new LiteralNode
            {
                Line = line,
                Value = 1,
                DataType = DataType.INTEGER
            };

            if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "STEP")
            {
                Advance(); // consume STEP
                step = ParseExpression();
            }

            SkipNewlines();
            List<IASTNode> body = ParseBlock(new[] { "NEXT" });

            Consume(TokenType.KEYWORD, "Expected NEXT to close FOR loop");
            if (Previous().Value.ToUpper() != "NEXT")
            {
                throw new Exception($"Expected NEXT to close FOR loop at line {line}");
            }

            string nextVar = Consume(TokenType.IDENTIFIER, "Expected variable name after NEXT").Value;
            if (nextVar != variable)
            {
                throw new Exception($"NEXT identifier does not match FOR at line {Previous().Line}");
            }

            return new ForNode
            {
                Line = line,
                Variable = variable,
                Start = start,
                End = end,
                Step = step,
                Body = body
            };
        }

        private CaseNode ParseCase()
        {
            int line = Advance().Line; // consume CASE

            Consume(TokenType.KEYWORD, "Expected OF after CASE");
            if (Previous().Value.ToUpper() != "OF")
            {
                throw new Exception($"Expected OF after CASE at line {line}");
            }

            IExpressionNode expression = ParseExpression();

            SkipNewlines();

            var cases = new List<CaseOption>();
            List<IASTNode>? otherwiseBlock = null;

            while (!Check(TokenType.KEYWORD) || Peek().Value.ToUpper() != "ENDCASE")
            {
                SkipNewlines();

                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "OTHERWISE")
                {
                    Advance(); // consume OTHERWISE
                    Consume(TokenType.COLON, "Expected : after OTHERWISE");
                    SkipNewlines();
                    otherwiseBlock = ParseBlock(new[] { "ENDCASE" });
                    break;
                }

                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "ENDCASE")
                {
                    break;
                }

                IExpressionNode caseValue = ParseExpression();

                // Check for range (value TO value)
                IExpressionNode? rangeStart = null;
                IExpressionNode? rangeEnd = null;
                IExpressionNode? singleValue = null;

                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "TO")
                {
                    Advance(); // consume TO
                    rangeStart = caseValue;
                    rangeEnd = ParseExpression();
                }
                else
                {
                    singleValue = caseValue;
                }

                Consume(TokenType.COLON, "Expected : after case value");
                SkipNewlines();

                var statements = new List<IASTNode>();
                while (!IsAtEnd())
                {
                    SkipNewlines();
                    if (IsAtEnd()) break;

                    // Check if next line starts with a case value or OTHERWISE/ENDCASE
                    if (Check(TokenType.NUMBER) || Check(TokenType.STRING) ||
                        (Check(TokenType.KEYWORD) && (Peek().Value.ToUpper() == "TRUE" || Peek().Value.ToUpper() == "FALSE" ||
                                                       Peek().Value.ToUpper() == "OTHERWISE" || Peek().Value.ToUpper() == "ENDCASE")))
                    {
                        break;
                    }

                    // Special handling for DECLARE
                    if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "DECLARE")
                    {
                        var declareNodes = ParseDeclareStatements();
                        statements.AddRange(declareNodes);
                    }
                    else
                    {
                        var stmt = ParseStatement();
                        if (stmt != null)
                        {
                            statements.Add(stmt);
                        }
                    }
                    SkipNewlines();
                }

                cases.Add(new CaseOption
                {
                    Value = singleValue,
                    RangeStart = rangeStart,
                    RangeEnd = rangeEnd,
                    Statements = statements
                });
            }

            Consume(TokenType.KEYWORD, "Expected ENDCASE to close CASE statement");
            if (Previous().Value.ToUpper() != "ENDCASE")
            {
                throw new Exception($"Expected ENDCASE to close CASE statement at line {line}");
            }

            return new CaseNode
            {
                Line = line,
                Expression = expression,
                Cases = cases,
                OtherwiseBlock = otherwiseBlock
            };
        }

        private ProcedureNode ParseProcedure()
        {
            int line = Advance().Line; // consume PROCEDURE

            string name = Consume(TokenType.IDENTIFIER, "Expected procedure name").Value;
            Consume(TokenType.LPAREN, "Expected ( after procedure name");

            List<Parameter> parameters = ParseParameters();

            Consume(TokenType.RPAREN, "Expected ) after parameters");

            SkipNewlines();
            List<IASTNode> body = ParseBlock(new[] { "ENDPROCEDURE" });

            Consume(TokenType.KEYWORD, "Expected ENDPROCEDURE");
            if (Previous().Value.ToUpper() != "ENDPROCEDURE")
            {
                throw new Exception($"Expected ENDPROCEDURE at line {line}");
            }

            return new ProcedureNode
            {
                Line = line,
                Name = name,
                Parameters = parameters,
                Body = body
            };
        }

        private FunctionNode ParseFunction()
        {
            int line = Advance().Line; // consume FUNCTION

            string name = Consume(TokenType.IDENTIFIER, "Expected function name").Value;
            Consume(TokenType.LPAREN, "Expected ( after function name");

            List<Parameter> parameters = ParseParameters();

            Consume(TokenType.RPAREN, "Expected ) after parameters");

            Consume(TokenType.KEYWORD, "Expected RETURNS after function parameters");
            if (Previous().Value.ToUpper() != "RETURNS")
            {
                throw new Exception($"Expected RETURNS after function parameters at line {line}");
            }

            DataType returnType = ParseDataType();

            SkipNewlines();
            List<IASTNode> body = ParseBlock(new[] { "ENDFUNCTION" });

            Consume(TokenType.KEYWORD, "Expected ENDFUNCTION");
            if (Previous().Value.ToUpper() != "ENDFUNCTION")
            {
                throw new Exception($"Expected ENDFUNCTION at line {line}");
            }

            return new FunctionNode
            {
                Line = line,
                Name = name,
                Parameters = parameters,
                ReturnType = returnType,
                Body = body
            };
        }

        private CallNode ParseCall()
        {
            int line = Advance().Line; // consume CALL

            string name = Consume(TokenType.IDENTIFIER, "Expected procedure name after CALL").Value;
            Consume(TokenType.LPAREN, "Expected ( after procedure name");

            var args = new List<IExpressionNode>();

            if (!Check(TokenType.RPAREN))
            {
                while (true)
                {
                    args.Add(ParseExpression());
                    if (Check(TokenType.COMMA))
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Consume(TokenType.RPAREN, "Expected ) after arguments");

            return new CallNode
            {
                Line = line,
                Name = name,
                Arguments = args
            };
        }

        private ReturnNode ParseReturn()
        {
            int line = Advance().Line; // consume RETURN

            IExpressionNode value = ParseExpression();

            return new ReturnNode
            {
                Line = line,
                Value = value
            };
        }

        private OpenFileNode ParseOpenFile()
        {
            int line = Advance().Line; // consume OPENFILE

            IExpressionNode filename = ParseExpression();

            Consume(TokenType.KEYWORD, "Expected FOR after filename in OPENFILE");
            if (Previous().Value.ToUpper() != "FOR")
            {
                throw new Exception($"Expected FOR after filename in OPENFILE at line {line}");
            }

            Token modeToken = Consume(TokenType.KEYWORD, "Expected file mode (READ, WRITE, or APPEND) after FOR");
            string mode = modeToken.Value.ToUpper();

            if (mode != "READ" && mode != "WRITE" && mode != "APPEND")
            {
                throw new Exception($"Invalid file mode, expected READ, WRITE, or APPEND at line {modeToken.Line}");
            }

            FileMode fileMode = mode switch
            {
                "READ" => FileMode.READ,
                "WRITE" => FileMode.WRITE,
                "APPEND" => FileMode.APPEND,
                _ => throw new Exception($"Invalid file mode at line {modeToken.Line}")
            };

            return new OpenFileNode
            {
                Line = line,
                Filename = filename,
                Mode = fileMode
            };
        }

        private CloseFileNode ParseCloseFile()
        {
            int line = Advance().Line; // consume CLOSEFILE

            IExpressionNode filename = ParseExpression();

            return new CloseFileNode
            {
                Line = line,
                Filename = filename
            };
        }

        private ReadFileNode ParseReadFile()
        {
            int line = Advance().Line; // consume READFILE

            IExpressionNode filename = ParseExpression();

            Consume(TokenType.COMMA, "Expected , after filename in READFILE");

            string identifier = Consume(TokenType.IDENTIFIER, "Expected identifier after , in READFILE").Value;

            IExpressionNode target;

            if (Check(TokenType.LBRACKET))
            {
                // Array access
                Advance(); // consume [
                var indices = new List<IExpressionNode>();

                while (true)
                {
                    indices.Add(ParseExpression());
                    if (Check(TokenType.COMMA))
                    {
                        Advance();
                    }
                    else
                    {
                        break;
                    }
                }

                Consume(TokenType.RBRACKET, "Expected ] after array indices");

                target = new ArrayAccessNode
                {
                    Line = line,
                    Array = identifier,
                    Indices = indices
                };
            }
            else
            {
                target = new IdentifierNode
                {
                    Line = line,
                    Name = identifier
                };
            }

            return new ReadFileNode
            {
                Line = line,
                Filename = filename,
                Target = target
            };
        }

        private WriteFileNode ParseWriteFile()
        {
            int line = Advance().Line; // consume WRITEFILE

            IExpressionNode filename = ParseExpression();

            Consume(TokenType.COMMA, "Expected , after filename in WRITEFILE");

            IExpressionNode data = ParseExpression();

            return new WriteFileNode
            {
                Line = line,
                Filename = filename,
                Data = data
            };
        }

        private List<Parameter> ParseParameters()
        {
            var parameters = new List<Parameter>();

            if (Check(TokenType.RPAREN))
            {
                return parameters;
            }

            while (true)
            {
                bool byRef = false;

                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "BYREF")
                {
                    byRef = true;
                    Advance();
                }
                else if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "BYVAL")
                {
                    Advance();
                }

                string name = Consume(TokenType.IDENTIFIER, "Expected parameter name").Value;
                Consume(TokenType.COLON, "Expected : after parameter name");
                DataType type = ParseDataType();

                DataType? arrayElementType = null;

                // If type is ARRAY, check for "OF TYPE"
                if (type == DataType.ARRAY)
                {
                    Token ofToken = Consume(TokenType.KEYWORD, "Expected OF after ARRAY");
                    if (ofToken.Value.ToUpper() != "OF")
                    {
                        throw new Exception($"Expected OF after ARRAY at line {ofToken.Line}");
                    }
                    arrayElementType = ParseDataType();
                }

                parameters.Add(new Parameter
                {
                    Name = name,
                    Type = type,
                    ByRef = byRef,
                    ArrayElementType = arrayElementType
                });

                if (Check(TokenType.COMMA))
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            return parameters;
        }

        private DataType ParseDataType()
        {
            Token token = Consume(TokenType.KEYWORD, "Expected data type");
            string type = token.Value.ToUpper();

            if (new[] { "INTEGER", "REAL", "STRING", "CHAR", "BOOLEAN" }.Contains(type))
            {
                return type switch
                {
                    "INTEGER" => DataType.INTEGER,
                    "REAL" => DataType.REAL,
                    "STRING" => DataType.STRING,
                    "CHAR" => DataType.CHAR,
                    "BOOLEAN" => DataType.BOOLEAN,
                    _ => throw new Exception($"Invalid data type '{type}' at line {token.Line}")
                };
            }

            if (type == "ARRAY")
            {
                return DataType.ARRAY;
            }

            throw new Exception($"Invalid data type '{type}' at line {token.Line}");
        }

        private List<IASTNode> ParseBlock(string[] endKeywords)
        {
            var statements = new List<IASTNode>();
            var endKeywordsUpper = endKeywords.Select(k => k.ToUpper()).ToArray();

            while (!IsAtEnd())
            {
                SkipNewlines();

                if (Check(TokenType.KEYWORD) && endKeywordsUpper.Contains(Peek().Value.ToUpper()))
                {
                    break;
                }

                if (IsAtEnd()) break;

                // Special handling for DECLARE to support comma-separated identifiers
                if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "DECLARE")
                {
                    var declareNodes = ParseDeclareStatements();
                    statements.AddRange(declareNodes);
                }
                else
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                    {
                        statements.Add(stmt);
                    }
                }
            }

            return statements;
        }

        // Expression parsing with operator precedence
        private IExpressionNode ParseExpression()
        {
            return ParseOr();
        }

        private IExpressionNode ParseOr()
        {
            IExpressionNode expr = ParseAnd();

            while (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "OR")
            {
                Token op = Advance();
                IExpressionNode right = ParseAnd();
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = "OR",
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParseAnd()
        {
            IExpressionNode expr = ParseNot();

            while (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "AND")
            {
                Token op = Advance();
                IExpressionNode right = ParseNot();
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = "AND",
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParseNot()
        {
            if (Check(TokenType.KEYWORD) && Peek().Value.ToUpper() == "NOT")
            {
                Token op = Advance();
                IExpressionNode operand = ParseNot();
                return new UnaryOpNode
                {
                    Line = op.Line,
                    Operator = "NOT",
                    Operand = operand
                };
            }

            return ParseComparison();
        }

        private IExpressionNode ParseComparison()
        {
            IExpressionNode expr = ParseTerm();

            while (Check(TokenType.OPERATOR) && new[] { "=", "<>", "<", ">", "<=", ">=" }.Contains(Peek().Value))
            {
                Token op = Advance();
                IExpressionNode right = ParseTerm();
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = op.Value,
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParseTerm()
        {
            IExpressionNode expr = ParseFactor();

            while (Check(TokenType.OPERATOR) && new[] { "+", "-", "&" }.Contains(Peek().Value))
            {
                Token op = Advance();
                IExpressionNode right = ParseFactor();
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = op.Value,
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParseFactor()
        {
            IExpressionNode expr = ParsePower();

            while ((Check(TokenType.OPERATOR) && new[] { "*", "/" }.Contains(Peek().Value)) ||
                   (Check(TokenType.KEYWORD) && new[] { "DIV", "MOD" }.Contains(Peek().Value.ToUpper())))
            {
                Token op = Advance();
                IExpressionNode right = ParsePower();
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = op.Value.ToUpper(),
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParsePower()
        {
            IExpressionNode expr = ParseUnary();

            // Right-associative exponentiation
            if (Check(TokenType.OPERATOR) && Peek().Value == "^")
            {
                Token op = Advance();
                IExpressionNode right = ParsePower(); // Right-associative recursion
                expr = new BinaryOpNode
                {
                    Line = op.Line,
                    Operator = "^",
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        private IExpressionNode ParseUnary()
        {
            if (Check(TokenType.OPERATOR) && Peek().Value == "-")
            {
                Token op = Advance();
                IExpressionNode operand = ParseUnary();
                return new UnaryOpNode
                {
                    Line = op.Line,
                    Operator = "-",
                    Operand = operand
                };
            }

            return ParsePrimary();
        }

        private IExpressionNode ParsePrimary()
        {
            Token token = Peek();

            // Literals
            if (token.Type == TokenType.NUMBER)
            {
                Advance();
                object value = token.Value.Contains('.') ? (object)double.Parse(token.Value) : int.Parse(token.Value);
                DataType dataType = token.Value.Contains('.') ? DataType.REAL : DataType.INTEGER;
                return new LiteralNode
                {
                    Line = token.Line,
                    Value = value,
                    DataType = dataType
                };
            }

            if (token.Type == TokenType.STRING)
            {
                Advance();
                return new LiteralNode
                {
                    Line = token.Line,
                    Value = token.Value,
                    DataType = DataType.STRING
                };
            }

            if (token.Type == TokenType.KEYWORD && token.Value.ToUpper() == "TRUE")
            {
                Advance();
                return new LiteralNode
                {
                    Line = token.Line,
                    Value = true,
                    DataType = DataType.BOOLEAN
                };
            }

            if (token.Type == TokenType.KEYWORD && token.Value.ToUpper() == "FALSE")
            {
                Advance();
                return new LiteralNode
                {
                    Line = token.Line,
                    Value = false,
                    DataType = DataType.BOOLEAN
                };
            }

            // Parenthesized expression
            if (token.Type == TokenType.LPAREN)
            {
                Advance();
                IExpressionNode expr = ParseExpression();
                Consume(TokenType.RPAREN, "Expected ) after expression");
                return expr;
            }

            // Type conversion functions (INT, REAL, STRING are keywords but can be function calls)
            if (token.Type == TokenType.KEYWORD && new[] { "INT", "REAL", "STRING" }.Contains(token.Value.ToUpper()))
            {
                if (_current + 1 < _tokens.Count && _tokens[_current + 1].Type == TokenType.LPAREN)
                {
                    string name = Advance().Value;
                    Advance(); // consume (

                    var args = new List<IExpressionNode>();

                    if (!Check(TokenType.RPAREN))
                    {
                        while (true)
                        {
                            args.Add(ParseExpression());
                            if (Check(TokenType.COMMA))
                            {
                                Advance();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Consume(TokenType.RPAREN, "Expected ) after function arguments");

                    return new FunctionCallNode
                    {
                        Line = token.Line,
                        Name = name,
                        Arguments = args
                    };
                }
            }

            // Identifier, function call, or array access
            if (token.Type == TokenType.IDENTIFIER)
            {
                string name = Advance().Value;

                // Function call
                if (Check(TokenType.LPAREN))
                {
                    Advance(); // consume (

                    var args = new List<IExpressionNode>();

                    if (!Check(TokenType.RPAREN))
                    {
                        while (true)
                        {
                            args.Add(ParseExpression());
                            if (Check(TokenType.COMMA))
                            {
                                Advance();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Consume(TokenType.RPAREN, "Expected ) after function arguments");

                    return new FunctionCallNode
                    {
                        Line = token.Line,
                        Name = name,
                        Arguments = args
                    };
                }

                // Array access
                if (Check(TokenType.LBRACKET))
                {
                    Advance(); // consume [

                    var indices = new List<IExpressionNode>();

                    while (true)
                    {
                        indices.Add(ParseExpression());
                        if (Check(TokenType.COMMA))
                        {
                            Advance();
                        }
                        else
                        {
                            break;
                        }
                    }

                    Consume(TokenType.RBRACKET, "Expected ] after array indices");

                    return new ArrayAccessNode
                    {
                        Line = token.Line,
                        Array = name,
                        Indices = indices
                    };
                }

                // Simple identifier
                return new IdentifierNode
                {
                    Line = token.Line,
                    Name = name
                };
            }

            throw new Exception($"Unexpected token '{token.Value}' at line {token.Line}");
        }

        // Helper methods
        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }
            throw new Exception($"{message} at line {Peek().Line}");
        }

        private void SkipNewlines()
        {
            while (Check(TokenType.NEWLINE) && !IsAtEnd())
            {
                Advance();
            }
        }
    }
}

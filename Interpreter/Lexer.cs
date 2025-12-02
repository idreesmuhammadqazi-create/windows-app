using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PseudoRun.Desktop.Interpreter
{
    public class Lexer
    {
        private static readonly HashSet<string> KEYWORDS = new HashSet<string>
        {
            "DECLARE", "CONSTANT", "IF", "THEN", "ELSE", "ENDIF", "WHILE", "DO", "ENDWHILE",
            "REPEAT", "UNTIL", "FOR", "TO", "STEP", "NEXT", "CASE", "OF", "OTHERWISE", "ENDCASE",
            "INTEGER", "REAL", "STRING", "CHAR", "BOOLEAN", "ARRAY",
            "INPUT", "OUTPUT", "PROCEDURE", "ENDPROCEDURE", "FUNCTION", "ENDFUNCTION",
            "RETURN", "RETURNS", "CALL", "BYVAL", "BYREF", "TRUE", "FALSE",
            "AND", "OR", "NOT", "DIV", "MOD",
            "OPENFILE", "CLOSEFILE", "READFILE", "WRITEFILE", "EOF",
            "READ", "WRITE", "APPEND"
        };

        public List<Token> Tokenize(string code)
        {
            var tokens = new List<Token>();
            int line = 1;
            int column = 1;
            int i = 0;

            while (i < code.Length)
            {
                char c = code[i];

                // Skip whitespace except newlines
                if (c == ' ' || c == '\t' || c == '\r')
                {
                    i++;
                    column++;
                    continue;
                }

                // Newlines
                if (c == '\n')
                {
                    tokens.Add(new Token(TokenType.NEWLINE, "\n", line, column));
                    i++;
                    line++;
                    column = 1;
                    continue;
                }

                // Comments
                if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
                {
                    string comment = "";
                    int startColumn = column;
                    i += 2;
                    column += 2;
                    while (i < code.Length && code[i] != '\n')
                    {
                        comment += code[i];
                        i++;
                        column++;
                    }
                    tokens.Add(new Token(TokenType.COMMENT, comment, line, startColumn));
                    continue;
                }

                // String literals (double quotes)
                if (c == '"')
                {
                    string str = "";
                    int startColumn = column;
                    i++;
                    column++;
                    while (i < code.Length && code[i] != '"')
                    {
                        if (code[i] == '\n')
                        {
                            throw new Exception($"Unterminated string at line {line}, column {startColumn}");
                        }
                        str += code[i];
                        i++;
                        column++;
                    }
                    if (i >= code.Length)
                    {
                        throw new Exception($"Unterminated string at line {line}, column {startColumn}");
                    }
                    i++; // Skip closing quote
                    column++;
                    tokens.Add(new Token(TokenType.STRING, str, line, startColumn));
                    continue;
                }

                // Character literals (single quotes) - for CHAR type
                if (c == '\'')
                {
                    string str = "";
                    int startColumn = column;
                    i++;
                    column++;
                    while (i < code.Length && code[i] != '\'')
                    {
                        if (code[i] == '\n')
                        {
                            throw new Exception($"Unterminated character literal at line {line}, column {startColumn}");
                        }
                        str += code[i];
                        i++;
                        column++;
                    }
                    if (i >= code.Length)
                    {
                        throw new Exception($"Unterminated character literal at line {line}, column {startColumn}");
                    }
                    i++; // Skip closing quote
                    column++;
                    tokens.Add(new Token(TokenType.STRING, str, line, startColumn));
                    continue;
                }

                // Numbers
                if (char.IsDigit(c))
                {
                    string num = "";
                    int startColumn = column;
                    while (i < code.Length && (char.IsDigit(code[i]) || code[i] == '.'))
                    {
                        num += code[i];
                        i++;
                        column++;
                    }
                    tokens.Add(new Token(TokenType.NUMBER, num, line, startColumn));
                    continue;
                }

                // Assignment operator ← or <--
                if (c == '←' || (c == '<' && i + 2 < code.Length && code[i + 1] == '-' && code[i + 2] == '-'))
                {
                    int startColumn = column;
                    if (c == '←')
                    {
                        i++;
                        column++;
                    }
                    else
                    {
                        i += 3;
                        column += 3;
                    }
                    tokens.Add(new Token(TokenType.ASSIGNMENT, "<--", line, startColumn));
                    continue;
                }

                // Multi-character operators
                if (c == '<' && i + 1 < code.Length && code[i + 1] == '=')
                {
                    tokens.Add(new Token(TokenType.OPERATOR, "<=", line, column));
                    i += 2;
                    column += 2;
                    continue;
                }

                if (c == '>' && i + 1 < code.Length && code[i + 1] == '=')
                {
                    tokens.Add(new Token(TokenType.OPERATOR, ">=", line, column));
                    i += 2;
                    column += 2;
                    continue;
                }

                if (c == '<' && i + 1 < code.Length && code[i + 1] == '>')
                {
                    tokens.Add(new Token(TokenType.OPERATOR, "<>", line, column));
                    i += 2;
                    column += 2;
                    continue;
                }

                // Exponentiation operator ^
                if (c == '^')
                {
                    tokens.Add(new Token(TokenType.OPERATOR, "^", line, column));
                    i++;
                    column++;
                    continue;
                }

                // Single character operators
                if ("+-*/=<>&".Contains(c))
                {
                    tokens.Add(new Token(TokenType.OPERATOR, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                // Punctuation
                if (c == ',')
                {
                    tokens.Add(new Token(TokenType.COMMA, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                if (c == ':')
                {
                    tokens.Add(new Token(TokenType.COLON, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                if (c == '(')
                {
                    tokens.Add(new Token(TokenType.LPAREN, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                if (c == ')')
                {
                    tokens.Add(new Token(TokenType.RPAREN, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                if (c == '[')
                {
                    tokens.Add(new Token(TokenType.LBRACKET, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                if (c == ']')
                {
                    tokens.Add(new Token(TokenType.RBRACKET, c.ToString(), line, column));
                    i++;
                    column++;
                    continue;
                }

                // Identifiers and keywords
                if (char.IsLetter(c) || c == '_')
                {
                    string identifier = "";
                    int startColumn = column;
                    while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                    {
                        identifier += code[i];
                        i++;
                        column++;
                    }

                    // Check if it's a keyword (case-insensitive)
                    TokenType type = KEYWORDS.Contains(identifier.ToUpper()) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
                    tokens.Add(new Token(type, identifier, line, startColumn));
                    continue;
                }

                // Unknown character
                throw new Exception($"Unexpected character '{c}' at line {line}, column {column}");
            }

            // Add EOF token
            tokens.Add(new Token(TokenType.EOF, "", line, column));

            return tokens;
        }
    }
}

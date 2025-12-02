using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.Interpreter
{
    public class FileHandle
    {
        public FileMode Mode { get; set; }
        public List<string> Data { get; set; } = new List<string>();
        public int Position { get; set; }
    }

    public class ReturnValue : Exception
    {
        public object? Value { get; }

        public ReturnValue(object? value)
        {
            Value = value;
        }
    }

    public delegate Task<string> InputHandler(string variableName, string variableType);
    public delegate Task StepCallback();
    public delegate Task<string> FileUploadHandler(string filename);

    public class PseudocodeInterpreter
    {
        private const int MAX_ITERATIONS = 10000;
        private const int MAX_RECURSION_DEPTH = 1000;

        private ExecutionContext _globalContext;
        private int _iterationCount = 0;
        private int _recursionDepth = 0;
        private InputHandler _inputHandler;
        private bool _debugMode;
        private List<CallStackFrame> _callStack;
        private StepCallback? _stepCallback;
        private Dictionary<string, FileHandle> _fileHandles = new Dictionary<string, FileHandle>();
        private bool _fileWriteOutput;
        private FileUploadHandler? _fileUploadHandler;

        public PseudocodeInterpreter(
            InputHandler? inputHandler = null,
            bool debugMode = false,
            StepCallback? stepCallback = null,
            bool fileWriteOutput = true,
            FileUploadHandler? fileUploadHandler = null)
        {
            _globalContext = new ExecutionContext();
            _inputHandler = inputHandler ?? DefaultInputHandler;
            _debugMode = debugMode;
            _callStack = new List<CallStackFrame> { new CallStackFrame { Name = "main", Line = 1, Type = CallStackType.Main } };
            _stepCallback = stepCallback;
            _fileWriteOutput = fileWriteOutput;
            _fileUploadHandler = fileUploadHandler;
        }

        private Task<string> DefaultInputHandler(string variableName, string variableType)
        {
            // In WPF, this will be replaced with a proper dialog
            return Task.FromResult("0");
        }

        public DebugState GetDebugState()
        {
            return new DebugState
            {
                CurrentLine = _callStack.Count > 0 ? _callStack[_callStack.Count - 1].Line : 1,
                CallStack = new List<CallStackFrame>(_callStack),
                Variables = new Dictionary<string, Variable>(_globalContext.Variables),
                IsPaused = _debugMode,
                IsRunning = true
            };
        }

        public void DisableDebugMode()
        {
            _debugMode = false;
        }

        public string? GetFileContent(string filename)
        {
            if (!_fileHandles.TryGetValue(filename, out var fileHandle))
            {
                return null;
            }
            return string.Join("\n", fileHandle.Data);
        }

        public List<(string filename, string mode, int lineCount)> GetAllFiles()
        {
            var files = new List<(string, string, int)>();
            foreach (var kvp in _fileHandles)
            {
                files.Add((kvp.Key, kvp.Value.Mode.ToString(), kvp.Value.Data.Count));
            }
            return files;
        }

        public async IAsyncEnumerable<string> ExecuteProgram(
            List<IASTNode> ast,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var output in Execute(ast, cancellationToken))
            {
                yield return output;
            }
        }

        public async IAsyncEnumerable<string> Execute(
            List<IASTNode> ast,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // First pass: register procedures and functions
            foreach (var node in ast)
            {
                if (node is ProcedureNode procedure)
                {
                    _globalContext.Procedures[procedure.Name] = procedure;
                }
                else if (node is FunctionNode function)
                {
                    _globalContext.Functions[function.Name] = function;
                }
            }

            // Second pass: execute statements
            foreach (var node in ast)
            {
                if (node is not ProcedureNode && node is not FunctionNode)
                {
                    await foreach (var output in ExecuteNode(node, _globalContext, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
        }

        private async IAsyncEnumerable<string> ExecuteNode(
            IASTNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _iterationCount++;
            if (_iterationCount > MAX_ITERATIONS)
            {
                throw new RuntimeError("Execution timeout: Possible infinite loop", node.Line);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Debug mode: Pause before executing each statement
            if (_debugMode && _stepCallback != null)
            {
                // Update call stack with current line
                if (_callStack.Count > 0)
                {
                    _callStack[_callStack.Count - 1].Line = node.Line;
                }

                // Wait for step command
                await _stepCallback();
            }

            switch (node)
            {
                case DeclareNode declareNode:
                    ExecuteDeclare(declareNode, context);
                    break;
                case AssignmentNode assignmentNode:
                    await foreach (var output in ExecuteAssignment(assignmentNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case OutputNode outputNode:
                    await foreach (var output in ExecuteOutput(outputNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case InputNode inputNode:
                    await foreach (var output in ExecuteInput(inputNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case IfNode ifNode:
                    await foreach (var output in ExecuteIf(ifNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case WhileNode whileNode:
                    await foreach (var output in ExecuteWhile(whileNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case RepeatNode repeatNode:
                    await foreach (var output in ExecuteRepeat(repeatNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case ForNode forNode:
                    await foreach (var output in ExecuteFor(forNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case CaseNode caseNode:
                    await foreach (var output in ExecuteCase(caseNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case CallNode callNode:
                    await foreach (var output in ExecuteCall(callNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case ReturnNode returnNode:
                    var returnValue = EvaluateExpression(returnNode.Value, context);
                    throw new ReturnValue(returnValue);
                case OpenFileNode openFileNode:
                    await foreach (var output in ExecuteOpenFile(openFileNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case CloseFileNode closeFileNode:
                    await foreach (var output in ExecuteCloseFile(closeFileNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case ReadFileNode readFileNode:
                    await foreach (var output in ExecuteReadFile(readFileNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                case WriteFileNode writeFileNode:
                    await foreach (var output in ExecuteWriteFile(writeFileNode, context, cancellationToken))
                    {
                        yield return output;
                    }
                    break;
                default:
                    throw new RuntimeError($"Unknown node type: {node.Type}", node.Line);
            }
        }

        private void ExecuteDeclare(DeclareNode node, ExecutionContext context)
        {
            if (node.DataType == DataType.ARRAY)
            {
                var dimensions = node.ArrayBounds!.Dimensions;
                context.Variables[node.Identifier] = new Variable
                {
                    Type = DataType.ARRAY,
                    Value = CreateArray(dimensions),
                    Dimensions = dimensions,
                    ElementType = node.ArrayElementType,
                    Initialized = false
                };
            }
            else
            {
                context.Variables[node.Identifier] = new Variable
                {
                    Type = node.DataType,
                    Value = null,
                    Initialized = false
                };
            }
        }

        private object CreateArray(List<ArrayDimension> dimensions)
        {
            if (dimensions.Count == 1)
            {
                var dimension = dimensions[0];
                var arr = new Dictionary<int, (bool initialized, object? value)>();
                for (int i = dimension.Lower; i <= dimension.Upper; i++)
                {
                    arr[i] = (false, null);
                }
                return arr;
            }
            else
            {
                var dimension = dimensions[0];
                var arr = new Dictionary<int, object>();
                for (int i = dimension.Lower; i <= dimension.Upper; i++)
                {
                    arr[i] = CreateArray(dimensions.Skip(1).ToList());
                }
                return arr;
            }
        }

        private async IAsyncEnumerable<string> ExecuteAssignment(
            AssignmentNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var value = EvaluateExpression(node.Value, context);

            if (node.Target is IdentifierNode identifierNode)
            {
                var varName = identifierNode.Name;
                var variable = GetVariable(varName, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Variable '{varName}' not declared", node.Line);
                }

                variable.Value = value;
                variable.Initialized = true;
            }
            else if (node.Target is ArrayAccessNode arrayAccessNode)
            {
                var variable = GetVariable(arrayAccessNode.Array, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Array '{arrayAccessNode.Array}' not declared", node.Line);
                }

                if (variable.Type != DataType.ARRAY)
                {
                    throw new RuntimeError($"'{arrayAccessNode.Array}' is not an array", node.Line);
                }

                var indices = arrayAccessNode.Indices.Select(idx =>
                {
                    var val = EvaluateExpression(idx, context);
                    if (val is not int && val is not double)
                    {
                        throw new RuntimeError("Array index must be a number", node.Line);
                    }
                    return Convert.ToInt32(Math.Floor(Convert.ToDouble(val)));
                }).ToList();

                SetArrayElement(variable.Value, indices, value, variable.Dimensions!, node.Line);
            }

            yield break;
        }

        private void SetArrayElement(object? arr, List<int> indices, object? value, List<ArrayDimension> dimensions, int line)
        {
            if (arr == null) throw new RuntimeError("Array is null", line);

            if (indices.Count == 1)
            {
                var idx = indices[0];
                var dimension = dimensions[0];

                if (idx < dimension.Lower || idx > dimension.Upper)
                {
                    throw new RuntimeError("Array index out of bounds", line);
                }

                var dict = (Dictionary<int, (bool initialized, object? value)>)arr;
                dict[idx] = (true, value);
            }
            else
            {
                var idx = indices[0];
                var dimension = dimensions[0];

                if (idx < dimension.Lower || idx > dimension.Upper)
                {
                    throw new RuntimeError("Array index out of bounds", line);
                }

                var dict = (Dictionary<int, object>)arr;
                SetArrayElement(dict[idx], indices.Skip(1).ToList(), value, dimensions.Skip(1).ToList(), line);
            }
        }

        private async IAsyncEnumerable<string> ExecuteOutput(
            OutputNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var parts = new List<string>();

            foreach (var expr in node.Expressions)
            {
                var value = EvaluateExpression(expr, context);
                parts.Add(ValueToString(value));
            }

            yield return string.Join(" ", parts);
        }

        private async IAsyncEnumerable<string> ExecuteInput(
            InputNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (node.Target is IdentifierNode identifierNode)
            {
                var varName = identifierNode.Name;
                var variable = GetVariable(varName, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Variable '{varName}' not declared", node.Line);
                }

                // Use the inputHandler to get input
                var input = await _inputHandler(varName, variable.Type.ToString());

                // Type conversion based on variable type
                object? value = variable.Type switch
                {
                    DataType.INTEGER => int.TryParse(input, out var i) ? i : 0,
                    DataType.REAL => double.TryParse(input, out var d) ? d : 0.0,
                    DataType.BOOLEAN => input.ToLower() == "true",
                    _ => input
                };

                variable.Value = value;
                variable.Initialized = true;

                // Echo the entered value to output
                yield return input;
            }
            else if (node.Target is ArrayAccessNode arrayAccessNode)
            {
                var variable = GetVariable(arrayAccessNode.Array, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Array '{arrayAccessNode.Array}' not declared", node.Line);
                }

                if (variable.Type != DataType.ARRAY)
                {
                    throw new RuntimeError($"'{arrayAccessNode.Array}' is not an array", node.Line);
                }

                var indices = arrayAccessNode.Indices.Select(idx =>
                {
                    var val = EvaluateExpression(idx, context);
                    if (val is not int && val is not double)
                    {
                        throw new RuntimeError("Array index must be a number", node.Line);
                    }
                    return Convert.ToInt32(Math.Floor(Convert.ToDouble(val)));
                }).ToList();

                // Determine the type to prompt for based on array element type
                var elementType = variable.ElementType ?? DataType.STRING;
                var promptName = $"{arrayAccessNode.Array}[{string.Join(", ", indices)}]";

                // Use the inputHandler to get input
                var input = await _inputHandler(promptName, elementType.ToString());

                // Type conversion based on element type
                object? value = elementType switch
                {
                    DataType.INTEGER => int.TryParse(input, out var i) ? i : 0,
                    DataType.REAL => double.TryParse(input, out var d) ? d : 0.0,
                    DataType.BOOLEAN => input.ToLower() == "true",
                    _ => input
                };

                SetArrayElement(variable.Value, indices, value, variable.Dimensions!, node.Line);

                // Echo the entered value to output
                yield return input;
            }
        }

        private async IAsyncEnumerable<string> ExecuteIf(
            IfNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var condition = EvaluateExpression(node.Condition, context);

            if (condition is not bool)
            {
                throw new RuntimeError("IF condition must be boolean", node.Line);
            }

            if ((bool)condition)
            {
                foreach (var stmt in node.ThenBlock)
                {
                    await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
            else if (node.ElseIfBlocks != null)
            {
                bool executed = false;
                foreach (var elseIfBlock in node.ElseIfBlocks)
                {
                    var elseIfCondition = EvaluateExpression(elseIfBlock.Condition, context);

                    if (elseIfCondition is not bool)
                    {
                        throw new RuntimeError("ELSE IF condition must be boolean", node.Line);
                    }

                    if ((bool)elseIfCondition)
                    {
                        foreach (var stmt in elseIfBlock.Block)
                        {
                            await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                            {
                                yield return output;
                            }
                        }
                        executed = true;
                        break;
                    }
                }

                if (!executed && node.ElseBlock != null)
                {
                    foreach (var stmt in node.ElseBlock)
                    {
                        await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                        {
                            yield return output;
                        }
                    }
                }
            }
            else if (node.ElseBlock != null)
            {
                foreach (var stmt in node.ElseBlock)
                {
                    await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
        }

        private async IAsyncEnumerable<string> ExecuteWhile(
            WhileNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var condition = EvaluateExpression(node.Condition, context);

                if (condition is not bool)
                {
                    throw new RuntimeError("WHILE condition must be boolean", node.Line);
                }

                if (!(bool)condition) break;

                foreach (var stmt in node.Body)
                {
                    await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
        }

        private async IAsyncEnumerable<string> ExecuteRepeat(
            RepeatNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            do
            {
                foreach (var stmt in node.Body)
                {
                    await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                    {
                        yield return output;
                    }
                }

                var condition = EvaluateExpression(node.Condition, context);

                if (condition is not bool)
                {
                    throw new RuntimeError("UNTIL condition must be boolean", node.Line);
                }

                if ((bool)condition) break;
            } while (true);
        }

        private async IAsyncEnumerable<string> ExecuteFor(
            ForNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var variable = GetVariable(node.Variable, context);

            // Auto-declare loop variable if it doesn't exist (implicit INTEGER type)
            if (variable == null)
            {
                context.Variables[node.Variable] = new Variable
                {
                    Type = DataType.INTEGER,
                    Value = 0,
                    Initialized = false
                };
                variable = context.Variables[node.Variable];
            }

            var start = EvaluateExpression(node.Start, context);
            var end = EvaluateExpression(node.End, context);
            var step = EvaluateExpression(node.Step, context);

            if (start is not int && start is not double ||
                end is not int && end is not double ||
                step is not int && step is not double)
            {
                throw new RuntimeError("FOR loop bounds must be numbers", node.Line);
            }

            var startNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(start)));
            var endNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(end)));
            var stepNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(step)));

            if (stepNum == 0)
            {
                throw new RuntimeError("FOR loop STEP cannot be zero", node.Line);
            }

            variable.Value = startNum;
            variable.Initialized = true;

            if (stepNum > 0)
            {
                while (Convert.ToInt32(variable.Value) <= endNum)
                {
                    foreach (var stmt in node.Body)
                    {
                        await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                        {
                            yield return output;
                        }
                    }
                    variable.Value = Convert.ToInt32(variable.Value) + stepNum;
                }
            }
            else
            {
                while (Convert.ToInt32(variable.Value) >= endNum)
                {
                    foreach (var stmt in node.Body)
                    {
                        await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                        {
                            yield return output;
                        }
                    }
                    variable.Value = Convert.ToInt32(variable.Value) + stepNum;
                }
            }
        }

        private async IAsyncEnumerable<string> ExecuteCase(
            CaseNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var value = EvaluateExpression(node.Expression, context);

            foreach (var caseBlock in node.Cases)
            {
                bool matched = false;

                if (caseBlock.Value != null)
                {
                    // Single value case
                    var caseValue = EvaluateExpression(caseBlock.Value, context);
                    matched = Equals(value, caseValue);
                }
                else if (caseBlock.RangeStart != null && caseBlock.RangeEnd != null)
                {
                    // Range case (value TO value)
                    var rangeStart = EvaluateExpression(caseBlock.RangeStart, context);
                    var rangeEnd = EvaluateExpression(caseBlock.RangeEnd, context);

                    // Convert to numbers for comparison
                    var numValue = Convert.ToDouble(value);
                    var numStart = Convert.ToDouble(rangeStart);
                    var numEnd = Convert.ToDouble(rangeEnd);

                    matched = (numValue >= numStart && numValue <= numEnd);
                }

                if (matched)
                {
                    foreach (var stmt in caseBlock.Statements)
                    {
                        await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                        {
                            yield return output;
                        }
                    }
                    yield break;
                }
            }

            if (node.OtherwiseBlock != null)
            {
                foreach (var stmt in node.OtherwiseBlock)
                {
                    await foreach (var output in ExecuteNode(stmt, context, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
        }

        private async IAsyncEnumerable<string> ExecuteCall(
            CallNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Check for procedure
            if (_globalContext.Procedures.TryGetValue(node.Name, out var procedure))
            {
                await foreach (var output in ExecuteProcedure(procedure, node.Arguments, context, node.Line, cancellationToken))
                {
                    yield return output;
                }
                yield break;
            }

            // Check for function (shouldn't be called with CALL, but handle it)
            if (_globalContext.Functions.TryGetValue(node.Name, out var func))
            {
                throw new RuntimeError($"Use assignment to call function '{node.Name}', not CALL", node.Line);
            }

            throw new RuntimeError($"Procedure '{node.Name}' not found", node.Line);
        }

        private async IAsyncEnumerable<string> ExecuteProcedure(
            ProcedureNode procedure,
            List<IExpressionNode> args,
            ExecutionContext callerContext,
            int callLine,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (args.Count != procedure.Parameters.Count)
            {
                throw new RuntimeError($"Incorrect number of arguments for procedure '{procedure.Name}'", callLine);
            }

            _recursionDepth++;
            if (_recursionDepth > MAX_RECURSION_DEPTH)
            {
                throw new RuntimeError("Maximum recursion depth exceeded", callLine);
            }

            var localContext = new ExecutionContext
            {
                Parent = _globalContext,
                Procedures = _globalContext.Procedures,
                Functions = _globalContext.Functions
            };

            // Bind parameters
            for (int i = 0; i < procedure.Parameters.Count; i++)
            {
                var param = procedure.Parameters[i];
                var arg = args[i];

                if (param.ByRef)
                {
                    if (arg is not IdentifierNode identifierNode)
                    {
                        throw new RuntimeError("BYREF parameter must be a variable", callLine);
                    }

                    var varName = identifierNode.Name;
                    var variable = GetVariable(varName, callerContext);

                    if (variable == null)
                    {
                        throw new RuntimeError($"Variable '{varName}' not found", callLine);
                    }

                    localContext.Variables[param.Name] = variable;
                }
                else
                {
                    var value = EvaluateExpression(arg, callerContext);
                    localContext.Variables[param.Name] = new Variable
                    {
                        Type = param.Type,
                        Value = value,
                        Initialized = true
                    };
                }
            }

            // Add to call stack
            _callStack.Add(new CallStackFrame { Name = procedure.Name, Line = callLine, Type = CallStackType.Procedure });

            try
            {
                // Execute procedure body
                foreach (var stmt in procedure.Body)
                {
                    await foreach (var output in ExecuteNode(stmt, localContext, cancellationToken))
                    {
                        yield return output;
                    }
                }
            }
            finally
            {
                _callStack.RemoveAt(_callStack.Count - 1);
                _recursionDepth--;
            }
        }

        private async IAsyncEnumerable<string> ExecuteOpenFile(
            OpenFileNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var filenameValue = EvaluateExpression(node.Filename, context);
            var filename = ValueToString(filenameValue);

            if (string.IsNullOrEmpty(filename))
            {
                throw new RuntimeError("Filename cannot be empty", node.Line);
            }

            if (_fileHandles.ContainsKey(filename))
            {
                throw new RuntimeError($"File '{filename}' is already open", node.Line);
            }

            var mode = node.Mode;
            FileHandle fileHandle;

            if (mode == FileMode.READ)
            {
                if (_fileUploadHandler == null)
                {
                    throw new RuntimeError("Cannot open file for reading: No file upload handler available", node.Line);
                }

                var content = await _fileUploadHandler(filename);
                var lines = content.Split('\n').ToList();

                fileHandle = new FileHandle
                {
                    Mode = FileMode.READ,
                    Data = lines,
                    Position = 0
                };
            }
            else if (mode == FileMode.WRITE)
            {
                fileHandle = new FileHandle
                {
                    Mode = FileMode.WRITE,
                    Data = new List<string>(),
                    Position = 0
                };
            }
            else // APPEND
            {
                if (_fileHandles.TryGetValue(filename, out var existingHandle))
                {
                    fileHandle = new FileHandle
                    {
                        Mode = FileMode.APPEND,
                        Data = existingHandle.Data,
                        Position = existingHandle.Data.Count
                    };
                }
                else
                {
                    fileHandle = new FileHandle
                    {
                        Mode = FileMode.APPEND,
                        Data = new List<string>(),
                        Position = 0
                    };
                }
            }

            _fileHandles[filename] = fileHandle;
            yield return $"Opened file '{filename}' in {mode} mode";
        }

        private async IAsyncEnumerable<string> ExecuteCloseFile(
            CloseFileNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var filenameValue = EvaluateExpression(node.Filename, context);
            var filename = ValueToString(filenameValue);

            if (string.IsNullOrEmpty(filename))
            {
                throw new RuntimeError("Filename cannot be empty", node.Line);
            }

            if (!_fileHandles.TryGetValue(filename, out var fileHandle))
            {
                throw new RuntimeError($"File '{filename}' is not open", node.Line);
            }

            if (fileHandle.Mode == FileMode.WRITE || fileHandle.Mode == FileMode.APPEND)
            {
                yield return $"Closed file '{filename}' ({fileHandle.Data.Count} lines written)";
                // Keep file in fileHandles for potential APPEND operations
            }
            else
            {
                _fileHandles.Remove(filename);
                yield return $"Closed file '{filename}'";
            }
        }

        private async IAsyncEnumerable<string> ExecuteReadFile(
            ReadFileNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var filenameValue = EvaluateExpression(node.Filename, context);
            var filename = ValueToString(filenameValue);

            if (string.IsNullOrEmpty(filename))
            {
                throw new RuntimeError("Filename cannot be empty", node.Line);
            }

            if (!_fileHandles.TryGetValue(filename, out var fileHandle))
            {
                throw new RuntimeError($"File '{filename}' is not open", node.Line);
            }

            if (fileHandle.Mode != FileMode.READ)
            {
                throw new RuntimeError($"File '{filename}' not opened for reading", node.Line);
            }

            if (fileHandle.Position >= fileHandle.Data.Count)
            {
                throw new RuntimeError($"Attempt to read past end of file '{filename}'", node.Line);
            }

            var line = fileHandle.Data[fileHandle.Position];
            fileHandle.Position++;

            // Store data in target variable (same logic as INPUT)
            if (node.Target is IdentifierNode identifierNode)
            {
                var varName = identifierNode.Name;
                var variable = GetVariable(varName, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Variable '{varName}' not declared", node.Line);
                }

                object? value = variable.Type switch
                {
                    DataType.INTEGER => int.TryParse(line, out var i) ? i : 0,
                    DataType.REAL => double.TryParse(line, out var d) ? d : 0.0,
                    DataType.BOOLEAN => line.ToLower() == "true",
                    _ => line
                };

                variable.Value = value;
                variable.Initialized = true;
            }
            else if (node.Target is ArrayAccessNode arrayAccessNode)
            {
                var variable = GetVariable(arrayAccessNode.Array, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Array '{arrayAccessNode.Array}' not declared", node.Line);
                }

                if (variable.Type != DataType.ARRAY)
                {
                    throw new RuntimeError($"'{arrayAccessNode.Array}' is not an array", node.Line);
                }

                var indices = arrayAccessNode.Indices.Select(idx =>
                {
                    var val = EvaluateExpression(idx, context);
                    if (val is not int && val is not double)
                    {
                        throw new RuntimeError("Array index must be a number", node.Line);
                    }
                    return Convert.ToInt32(Math.Floor(Convert.ToDouble(val)));
                }).ToList();

                var elementType = variable.ElementType ?? DataType.STRING;
                object? value = elementType switch
                {
                    DataType.INTEGER => int.TryParse(line, out var i) ? i : 0,
                    DataType.REAL => double.TryParse(line, out var d) ? d : 0.0,
                    DataType.BOOLEAN => line.ToLower() == "true",
                    _ => line
                };

                SetArrayElement(variable.Value, indices, value, variable.Dimensions!, node.Line);
            }

            yield return line;
        }

        private async IAsyncEnumerable<string> ExecuteWriteFile(
            WriteFileNode node,
            ExecutionContext context,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var filenameValue = EvaluateExpression(node.Filename, context);
            var filename = ValueToString(filenameValue);

            if (string.IsNullOrEmpty(filename))
            {
                throw new RuntimeError("Filename cannot be empty", node.Line);
            }

            if (!_fileHandles.TryGetValue(filename, out var fileHandle))
            {
                throw new RuntimeError($"File '{filename}' is not open", node.Line);
            }

            if (fileHandle.Mode == FileMode.READ)
            {
                throw new RuntimeError($"File '{filename}' not opened for writing", node.Line);
            }

            var dataValue = EvaluateExpression(node.Data, context);
            var dataString = ValueToString(dataValue);

            fileHandle.Data.Add(dataString);
            fileHandle.Position++;

            if (_fileWriteOutput)
            {
                yield return $"[Write to {filename}] {dataString}";
            }
        }

        private object? ExecuteFunction(
            FunctionNode func,
            List<IExpressionNode> args,
            ExecutionContext callerContext,
            int callLine)
        {
            if (args.Count != func.Parameters.Count)
            {
                throw new RuntimeError($"Incorrect number of arguments for function '{func.Name}'", callLine);
            }

            _recursionDepth++;
            if (_recursionDepth > MAX_RECURSION_DEPTH)
            {
                throw new RuntimeError("Maximum recursion depth exceeded", callLine);
            }

            var localContext = new ExecutionContext
            {
                Parent = _globalContext,
                Procedures = _globalContext.Procedures,
                Functions = _globalContext.Functions
            };

            // Bind parameters
            for (int i = 0; i < func.Parameters.Count; i++)
            {
                var param = func.Parameters[i];
                var arg = args[i];

                if (param.ByRef)
                {
                    if (arg is not IdentifierNode identifierNode)
                    {
                        throw new RuntimeError("BYREF parameter must be a variable", callLine);
                    }

                    var varName = identifierNode.Name;
                    var variable = GetVariable(varName, callerContext);

                    if (variable == null)
                    {
                        throw new RuntimeError($"Variable '{varName}' not found", callLine);
                    }

                    localContext.Variables[param.Name] = variable;
                }
                else
                {
                    var value = EvaluateExpression(arg, callerContext);
                    localContext.Variables[param.Name] = new Variable
                    {
                        Type = param.Type,
                        Value = value,
                        Initialized = true
                    };
                }
            }

            try
            {
                // Execute function body - sync only, no yields
                foreach (var stmt in func.Body)
                {
                    ExecuteSyncNode(stmt, localContext);
                }

                throw new RuntimeError($"Function '{func.Name}' did not return a value", callLine);
            }
            catch (ReturnValue returnValue)
            {
                return returnValue.Value;
            }
            finally
            {
                _recursionDepth--;
            }
        }

        private void ExecuteSyncNode(IASTNode node, ExecutionContext context)
        {
            switch (node)
            {
                case DeclareNode declareNode:
                    ExecuteDeclare(declareNode, context);
                    break;
                case AssignmentNode assignmentNode:
                    ExecuteSyncAssignment(assignmentNode, context);
                    break;
                case IfNode ifNode:
                    ExecuteSyncIf(ifNode, context);
                    break;
                case WhileNode whileNode:
                    ExecuteSyncWhile(whileNode, context);
                    break;
                case RepeatNode repeatNode:
                    ExecuteSyncRepeat(repeatNode, context);
                    break;
                case ForNode forNode:
                    ExecuteSyncFor(forNode, context);
                    break;
                case CaseNode caseNode:
                    ExecuteSyncCase(caseNode, context);
                    break;
                case ReturnNode returnNode:
                    var returnValue = EvaluateExpression(returnNode.Value, context);
                    throw new ReturnValue(returnValue);
                default:
                    throw new RuntimeError($"Unsupported statement in function: {node.Type}", node.Line);
            }
        }

        private void ExecuteSyncAssignment(AssignmentNode node, ExecutionContext context)
        {
            var value = EvaluateExpression(node.Value, context);

            if (node.Target is IdentifierNode identifierNode)
            {
                var varName = identifierNode.Name;
                var variable = GetVariable(varName, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Variable '{varName}' not declared", node.Line);
                }

                variable.Value = value;
                variable.Initialized = true;
            }
            else if (node.Target is ArrayAccessNode arrayAccessNode)
            {
                var variable = GetVariable(arrayAccessNode.Array, context);

                if (variable == null)
                {
                    throw new RuntimeError($"Array '{arrayAccessNode.Array}' not declared", node.Line);
                }

                if (variable.Type != DataType.ARRAY)
                {
                    throw new RuntimeError($"'{arrayAccessNode.Array}' is not an array", node.Line);
                }

                var indices = arrayAccessNode.Indices.Select(idx =>
                {
                    var val = EvaluateExpression(idx, context);
                    if (val is not int && val is not double)
                    {
                        throw new RuntimeError("Array index must be a number", node.Line);
                    }
                    return Convert.ToInt32(Math.Floor(Convert.ToDouble(val)));
                }).ToList();

                SetArrayElement(variable.Value, indices, value, variable.Dimensions!, node.Line);
            }
        }

        private void ExecuteSyncIf(IfNode node, ExecutionContext context)
        {
            var condition = EvaluateExpression(node.Condition, context);

            if (condition is not bool)
            {
                throw new RuntimeError("IF condition must be boolean", node.Line);
            }

            if ((bool)condition)
            {
                foreach (var stmt in node.ThenBlock)
                {
                    ExecuteSyncNode(stmt, context);
                }
            }
            else if (node.ElseIfBlocks != null)
            {
                bool executed = false;
                foreach (var elseIfBlock in node.ElseIfBlocks)
                {
                    var elseIfCondition = EvaluateExpression(elseIfBlock.Condition, context);

                    if (elseIfCondition is not bool)
                    {
                        throw new RuntimeError("ELSE IF condition must be boolean", node.Line);
                    }

                    if ((bool)elseIfCondition)
                    {
                        foreach (var stmt in elseIfBlock.Block)
                        {
                            ExecuteSyncNode(stmt, context);
                        }
                        executed = true;
                        break;
                    }
                }

                if (!executed && node.ElseBlock != null)
                {
                    foreach (var stmt in node.ElseBlock)
                    {
                        ExecuteSyncNode(stmt, context);
                    }
                }
            }
            else if (node.ElseBlock != null)
            {
                foreach (var stmt in node.ElseBlock)
                {
                    ExecuteSyncNode(stmt, context);
                }
            }
        }

        private void ExecuteSyncWhile(WhileNode node, ExecutionContext context)
        {
            while (true)
            {
                var condition = EvaluateExpression(node.Condition, context);

                if (condition is not bool)
                {
                    throw new RuntimeError("WHILE condition must be boolean", node.Line);
                }

                if (!(bool)condition) break;

                foreach (var stmt in node.Body)
                {
                    ExecuteSyncNode(stmt, context);
                }
            }
        }

        private void ExecuteSyncRepeat(RepeatNode node, ExecutionContext context)
        {
            do
            {
                foreach (var stmt in node.Body)
                {
                    ExecuteSyncNode(stmt, context);
                }

                var condition = EvaluateExpression(node.Condition, context);

                if (condition is not bool)
                {
                    throw new RuntimeError("UNTIL condition must be boolean", node.Line);
                }

                if ((bool)condition) break;
            } while (true);
        }

        private void ExecuteSyncFor(ForNode node, ExecutionContext context)
        {
            var variable = GetVariable(node.Variable, context);

            // Auto-declare loop variable if it doesn't exist
            if (variable == null)
            {
                context.Variables[node.Variable] = new Variable
                {
                    Type = DataType.INTEGER,
                    Value = 0,
                    Initialized = false
                };
                variable = context.Variables[node.Variable];
            }

            var start = EvaluateExpression(node.Start, context);
            var end = EvaluateExpression(node.End, context);
            var step = EvaluateExpression(node.Step, context);

            if (start is not int && start is not double ||
                end is not int && end is not double ||
                step is not int && step is not double)
            {
                throw new RuntimeError("FOR loop bounds must be numbers", node.Line);
            }

            var startNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(start)));
            var endNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(end)));
            var stepNum = Convert.ToInt32(Math.Floor(Convert.ToDouble(step)));

            if (stepNum == 0)
            {
                throw new RuntimeError("FOR loop STEP cannot be zero", node.Line);
            }

            variable.Value = startNum;
            variable.Initialized = true;

            if (stepNum > 0)
            {
                while (Convert.ToInt32(variable.Value) <= endNum)
                {
                    foreach (var stmt in node.Body)
                    {
                        ExecuteSyncNode(stmt, context);
                    }
                    variable.Value = Convert.ToInt32(variable.Value) + stepNum;
                }
            }
            else
            {
                while (Convert.ToInt32(variable.Value) >= endNum)
                {
                    foreach (var stmt in node.Body)
                    {
                        ExecuteSyncNode(stmt, context);
                    }
                    variable.Value = Convert.ToInt32(variable.Value) + stepNum;
                }
            }
        }

        private void ExecuteSyncCase(CaseNode node, ExecutionContext context)
        {
            var value = EvaluateExpression(node.Expression, context);

            foreach (var caseBlock in node.Cases)
            {
                bool matched = false;

                if (caseBlock.Value != null)
                {
                    var caseValue = EvaluateExpression(caseBlock.Value, context);
                    matched = Equals(value, caseValue);
                }
                else if (caseBlock.RangeStart != null && caseBlock.RangeEnd != null)
                {
                    var rangeStart = EvaluateExpression(caseBlock.RangeStart, context);
                    var rangeEnd = EvaluateExpression(caseBlock.RangeEnd, context);

                    var numValue = Convert.ToDouble(value);
                    var numStart = Convert.ToDouble(rangeStart);
                    var numEnd = Convert.ToDouble(rangeEnd);

                    matched = (numValue >= numStart && numValue <= numEnd);
                }

                if (matched)
                {
                    foreach (var stmt in caseBlock.Statements)
                    {
                        ExecuteSyncNode(stmt, context);
                    }
                    return;
                }
            }

            if (node.OtherwiseBlock != null)
            {
                foreach (var stmt in node.OtherwiseBlock)
                {
                    ExecuteSyncNode(stmt, context);
                }
            }
        }

        private object? EvaluateExpression(IExpressionNode expr, ExecutionContext context)
        {
            return expr switch
            {
                LiteralNode literalNode => literalNode.Value,
                IdentifierNode identifierNode => EvaluateIdentifier(identifierNode, context),
                ArrayAccessNode arrayAccessNode => EvaluateArrayAccess(arrayAccessNode, context),
                BinaryOpNode binaryOpNode => EvaluateBinaryOp(binaryOpNode, context),
                UnaryOpNode unaryOpNode => EvaluateUnaryOp(unaryOpNode, context),
                FunctionCallNode functionCallNode => EvaluateFunctionCall(functionCallNode, context),
                _ => throw new RuntimeError($"Unknown expression type: {expr.Type}", expr.Line)
            };
        }

        private object? EvaluateIdentifier(IdentifierNode node, ExecutionContext context)
        {
            var variable = GetVariable(node.Name, context);

            if (variable == null)
            {
                throw new RuntimeError($"Variable '{node.Name}' not declared", node.Line);
            }

            if (!variable.Initialized)
            {
                throw new RuntimeError($"Variable '{node.Name}' used before assignment", node.Line);
            }

            return variable.Value;
        }

        private object? EvaluateArrayAccess(ArrayAccessNode node, ExecutionContext context)
        {
            var variable = GetVariable(node.Array, context);

            if (variable == null)
            {
                throw new RuntimeError($"Array '{node.Array}' not declared", node.Line);
            }

            if (variable.Type != DataType.ARRAY)
            {
                throw new RuntimeError($"'{node.Array}' is not an array", node.Line);
            }

            var indices = node.Indices.Select(idx =>
            {
                var val = EvaluateExpression(idx, context);
                if (val is not int && val is not double)
                {
                    throw new RuntimeError("Array index must be a number", node.Line);
                }
                return Convert.ToInt32(Math.Floor(Convert.ToDouble(val)));
            }).ToList();

            return GetArrayElement(variable.Value, indices, variable.Dimensions!, node.Line);
        }

        private object? GetArrayElement(object? arr, List<int> indices, List<ArrayDimension> dimensions, int line)
        {
            if (arr == null) throw new RuntimeError("Array is null", line);

            if (indices.Count == 1)
            {
                var idx = indices[0];
                var dimension = dimensions[0];

                if (idx < dimension.Lower || idx > dimension.Upper)
                {
                    throw new RuntimeError("Array index out of bounds", line);
                }

                var dict = (Dictionary<int, (bool initialized, object? value)>)arr;
                var element = dict[idx];
                if (!element.initialized)
                {
                    throw new RuntimeError("Array element accessed before assignment", line);
                }

                return element.value;
            }
            else
            {
                var idx = indices[0];
                var dimension = dimensions[0];

                if (idx < dimension.Lower || idx > dimension.Upper)
                {
                    throw new RuntimeError("Array index out of bounds", line);
                }

                var dict = (Dictionary<int, object>)arr;
                return GetArrayElement(dict[idx], indices.Skip(1).ToList(), dimensions.Skip(1).ToList(), line);
            }
        }

        private object? EvaluateBinaryOp(BinaryOpNode node, ExecutionContext context)
        {
            var left = EvaluateExpression(node.Left, context);
            var right = EvaluateExpression(node.Right, context);

            switch (node.Operator)
            {
                case "+":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) + Convert.ToDouble(right);
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "-":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) - Convert.ToDouble(right);
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "*":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) * Convert.ToDouble(right);
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "/":
                    if (left is int || left is double && right is int || right is double)
                    {
                        var rightNum = Convert.ToDouble(right);
                        if (rightNum == 0)
                        {
                            throw new RuntimeError("Division by zero", node.Line);
                        }
                        return Convert.ToDouble(left) / rightNum;
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "^":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "DIV":
                    if (left is int || left is double && right is int || right is double)
                    {
                        var rightNum = Convert.ToDouble(right);
                        if (rightNum == 0)
                        {
                            throw new RuntimeError("Division by zero", node.Line);
                        }
                        return Math.Floor(Convert.ToDouble(left) / rightNum);
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "MOD":
                    if (left is int || left is double && right is int || right is double)
                    {
                        var rightNum = Convert.ToDouble(right);
                        if (rightNum == 0)
                        {
                            throw new RuntimeError("Division by zero", node.Line);
                        }
                        return Convert.ToDouble(left) % rightNum;
                    }
                    throw new RuntimeError("Cannot perform arithmetic on non-numbers", node.Line);

                case "&":
                    return ValueToString(left) + ValueToString(right);

                case "=":
                    return Equals(left, right);

                case "<>":
                    return !Equals(left, right);

                case "<":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) < Convert.ToDouble(right);
                    }
                    if (left is string && right is string)
                    {
                        return string.Compare((string)left, (string)right) < 0;
                    }
                    throw new RuntimeError("Invalid comparison", node.Line);

                case ">":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) > Convert.ToDouble(right);
                    }
                    if (left is string && right is string)
                    {
                        return string.Compare((string)left, (string)right) > 0;
                    }
                    throw new RuntimeError("Invalid comparison", node.Line);

                case "<=":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) <= Convert.ToDouble(right);
                    }
                    if (left is string && right is string)
                    {
                        return string.Compare((string)left, (string)right) <= 0;
                    }
                    throw new RuntimeError("Invalid comparison", node.Line);

                case ">=":
                    if (left is int || left is double && right is int || right is double)
                    {
                        return Convert.ToDouble(left) >= Convert.ToDouble(right);
                    }
                    if (left is string && right is string)
                    {
                        return string.Compare((string)left, (string)right) >= 0;
                    }
                    throw new RuntimeError("Invalid comparison", node.Line);

                case "AND":
                    if (left is bool && right is bool)
                    {
                        return (bool)left && (bool)right;
                    }
                    throw new RuntimeError("Logical operators require boolean operands", node.Line);

                case "OR":
                    if (left is bool && right is bool)
                    {
                        return (bool)left || (bool)right;
                    }
                    throw new RuntimeError("Logical operators require boolean operands", node.Line);

                default:
                    throw new RuntimeError($"Unknown operator: {node.Operator}", node.Line);
            }
        }

        private object? EvaluateUnaryOp(UnaryOpNode node, ExecutionContext context)
        {
            var operand = EvaluateExpression(node.Operand, context);

            switch (node.Operator)
            {
                case "-":
                    if (operand is int || operand is double)
                    {
                        return -Convert.ToDouble(operand);
                    }
                    throw new RuntimeError("Cannot negate non-number", node.Line);

                case "NOT":
                    if (operand is bool)
                    {
                        return !(bool)operand;
                    }
                    throw new RuntimeError("NOT requires boolean operand", node.Line);

                default:
                    throw new RuntimeError($"Unknown unary operator: {node.Operator}", node.Line);
            }
        }

        private object? EvaluateFunctionCall(FunctionCallNode node, ExecutionContext context)
        {
            // Check for built-in functions
            var builtIn = EvaluateBuiltInFunction(node.Name, node.Arguments, context, node.Line);
            if (builtIn != null)
            {
                return builtIn;
            }

            // Check for user-defined function
            if (_globalContext.Functions.TryGetValue(node.Name, out var func))
            {
                return ExecuteFunction(func, node.Arguments, context, node.Line);
            }

            throw new RuntimeError($"Function '{node.Name}' not found", node.Line);
        }

        private object? EvaluateBuiltInFunction(string name, List<IExpressionNode> args, ExecutionContext context, int line)
        {
            switch (name.ToUpper())
            {
                case "LENGTH":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("LENGTH requires 1 parameter", line);
                    }
                    var str = EvaluateExpression(args[0], context);
                    if (str is not string)
                    {
                        throw new RuntimeError("LENGTH requires string parameter", line);
                    }
                    return ((string)str).Length;

                case "SUBSTRING":
                    if (args.Count != 3)
                    {
                        throw new RuntimeError("SUBSTRING requires 3 parameters", line);
                    }
                    var substr = EvaluateExpression(args[0], context);
                    var start = EvaluateExpression(args[1], context);
                    var length = EvaluateExpression(args[2], context);

                    if (substr is not string)
                    {
                        throw new RuntimeError("SUBSTRING parameter type mismatch", line);
                    }
                    if (start is not int && start is not double || length is not int && length is not double)
                    {
                        throw new RuntimeError("SUBSTRING parameter type mismatch", line);
                    }

                    var startInt = Convert.ToInt32(start);
                    var lengthInt = Convert.ToInt32(length);

                    if (startInt < 1)
                    {
                        throw new RuntimeError("SUBSTRING start position must be >= 1", line);
                    }
                    if (lengthInt < 0)
                    {
                        throw new RuntimeError("SUBSTRING length cannot be negative", line);
                    }

                    return ((string)substr).Substring(startInt - 1, Math.Min(lengthInt, ((string)substr).Length - startInt + 1));

                case "UCASE":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("UCASE requires 1 parameter", line);
                    }
                    var ucaseStr = EvaluateExpression(args[0], context);
                    if (ucaseStr is not string)
                    {
                        throw new RuntimeError("UCASE requires string parameter", line);
                    }
                    return ((string)ucaseStr).ToUpper();

                case "LCASE":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("LCASE requires 1 parameter", line);
                    }
                    var lcaseStr = EvaluateExpression(args[0], context);
                    if (lcaseStr is not string)
                    {
                        throw new RuntimeError("LCASE requires string parameter", line);
                    }
                    return ((string)lcaseStr).ToLower();

                case "INT":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("INT requires 1 parameter", line);
                    }
                    var intVal = EvaluateExpression(args[0], context);
                    if (intVal is int || intVal is double)
                    {
                        return (int)Math.Floor(Convert.ToDouble(intVal));
                    }
                    if (intVal is string)
                    {
                        return int.TryParse((string)intVal, out var parsed) ? parsed : 0;
                    }
                    if (intVal is bool)
                    {
                        return (bool)intVal ? 1 : 0;
                    }
                    return 0;

                case "REAL":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("REAL requires 1 parameter", line);
                    }
                    var realVal = EvaluateExpression(args[0], context);
                    if (realVal is int || realVal is double)
                    {
                        return Convert.ToDouble(realVal);
                    }
                    if (realVal is string)
                    {
                        return double.TryParse((string)realVal, out var parsed) ? parsed : 0.0;
                    }
                    if (realVal is bool)
                    {
                        return (bool)realVal ? 1.0 : 0.0;
                    }
                    return 0.0;

                case "STRING":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("STRING requires 1 parameter", line);
                    }
                    var strVal = EvaluateExpression(args[0], context);
                    return ValueToString(strVal);

                case "ROUND":
                    if (args.Count != 2)
                    {
                        throw new RuntimeError("ROUND requires 2 parameters", line);
                    }
                    var roundVal = EvaluateExpression(args[0], context);
                    var decimals = EvaluateExpression(args[1], context);

                    if (roundVal is not int && roundVal is not double || decimals is not int && decimals is not double)
                    {
                        throw new RuntimeError("ROUND parameter type mismatch", line);
                    }

                    var multiplier = Math.Pow(10, Convert.ToInt32(decimals));
                    return Math.Round(Convert.ToDouble(roundVal) * multiplier) / multiplier;

                case "RANDOM":
                    if (args.Count != 0)
                    {
                        throw new RuntimeError("RANDOM takes no parameters", line);
                    }
                    return new Random().NextDouble();

                case "EOF":
                    if (args.Count != 1)
                    {
                        throw new RuntimeError("EOF requires 1 parameter", line);
                    }
                    var filename = ValueToString(EvaluateExpression(args[0], context));

                    if (string.IsNullOrEmpty(filename))
                    {
                        throw new RuntimeError("Filename cannot be empty", line);
                    }

                    if (!_fileHandles.TryGetValue(filename, out var fileHandle))
                    {
                        throw new RuntimeError($"File '{filename}' is not open", line);
                    }

                    if (fileHandle.Mode != FileMode.READ)
                    {
                        throw new RuntimeError("EOF can only be used with files opened for reading", line);
                    }

                    return fileHandle.Position >= fileHandle.Data.Count;

                default:
                    return null;
            }
        }

        private Variable? GetVariable(string name, ExecutionContext context)
        {
            var current = context;

            while (current != null)
            {
                if (current.Variables.TryGetValue(name, out var variable))
                {
                    return variable;
                }
                current = current.Parent;
            }

            return null;
        }

        private string ValueToString(object? value)
        {
            if (value is bool b)
            {
                return b ? "TRUE" : "FALSE";
            }
            if (value == null)
            {
                return "";
            }
            return value.ToString() ?? "";
        }
    }
}

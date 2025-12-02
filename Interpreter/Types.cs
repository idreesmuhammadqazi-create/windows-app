using System;
using System.Collections.Generic;

namespace PseudoRun.Desktop.Interpreter
{
    // Token types for lexer
    public enum TokenType
    {
        KEYWORD,
        IDENTIFIER,
        NUMBER,
        STRING,
        OPERATOR,
        ASSIGNMENT,
        COMMA,
        COLON,
        LPAREN,
        RPAREN,
        LBRACKET,
        RBRACKET,
        NEWLINE,
        EOF,
        COMMENT
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
    }

    // Data types
    public enum DataType
    {
        INTEGER,
        REAL,
        STRING,
        CHAR,
        BOOLEAN,
        ARRAY
    }

    // File operation modes
    public enum FileMode
    {
        READ,
        WRITE,
        APPEND
    }

    // AST Node base interface
    public interface IASTNode
    {
        string Type { get; }
        int Line { get; }
    }

    // Expression Node interface (subset of ASTNode)
    public interface IExpressionNode : IASTNode
    {
    }

    // Array bounds
    public class ArrayBounds
    {
        public List<ArrayDimension> Dimensions { get; set; } = new List<ArrayDimension>();
    }

    public class ArrayDimension
    {
        public int Lower { get; set; }
        public int Upper { get; set; }

        public ArrayDimension(int lower, int upper)
        {
            Lower = lower;
            Upper = upper;
        }
    }

    // Statement Nodes

    public class DeclareNode : IASTNode
    {
        public string Type => "Declare";
        public int Line { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public DataType DataType { get; set; }
        public ArrayBounds? ArrayBounds { get; set; }
        public DataType? ArrayElementType { get; set; }
    }

    public class AssignmentNode : IASTNode
    {
        public string Type => "Assignment";
        public int Line { get; set; }
        public IExpressionNode Target { get; set; } = null!;
        public IExpressionNode Value { get; set; } = null!;
    }

    public class OutputNode : IASTNode
    {
        public string Type => "Output";
        public int Line { get; set; }
        public List<IExpressionNode> Expressions { get; set; } = new List<IExpressionNode>();
    }

    public class InputNode : IASTNode
    {
        public string Type => "Input";
        public int Line { get; set; }
        public IExpressionNode Target { get; set; } = null!;
    }

    public class IfNode : IASTNode
    {
        public string Type => "If";
        public int Line { get; set; }
        public IExpressionNode Condition { get; set; } = null!;
        public List<IASTNode> ThenBlock { get; set; } = new List<IASTNode>();
        public List<ElseIfBlock>? ElseIfBlocks { get; set; }
        public List<IASTNode>? ElseBlock { get; set; }
    }

    public class ElseIfBlock
    {
        public IExpressionNode Condition { get; set; } = null!;
        public List<IASTNode> Block { get; set; } = new List<IASTNode>();
    }

    public class WhileNode : IASTNode
    {
        public string Type => "While";
        public int Line { get; set; }
        public IExpressionNode Condition { get; set; } = null!;
        public List<IASTNode> Body { get; set; } = new List<IASTNode>();
    }

    public class RepeatNode : IASTNode
    {
        public string Type => "Repeat";
        public int Line { get; set; }
        public List<IASTNode> Body { get; set; } = new List<IASTNode>();
        public IExpressionNode Condition { get; set; } = null!;
    }

    public class ForNode : IASTNode
    {
        public string Type => "For";
        public int Line { get; set; }
        public string Variable { get; set; } = string.Empty;
        public IExpressionNode Start { get; set; } = null!;
        public IExpressionNode End { get; set; } = null!;
        public IExpressionNode Step { get; set; } = null!;
        public List<IASTNode> Body { get; set; } = new List<IASTNode>();
    }

    public class CaseNode : IASTNode
    {
        public string Type => "Case";
        public int Line { get; set; }
        public IExpressionNode Expression { get; set; } = null!;
        public List<CaseOption> Cases { get; set; } = new List<CaseOption>();
        public List<IASTNode>? OtherwiseBlock { get; set; }
    }

    public class CaseOption
    {
        public IExpressionNode? Value { get; set; }
        public IExpressionNode? RangeStart { get; set; }
        public IExpressionNode? RangeEnd { get; set; }
        public List<IASTNode> Statements { get; set; } = new List<IASTNode>();
    }

    public class ProcedureNode : IASTNode
    {
        public string Type => "Procedure";
        public int Line { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public List<IASTNode> Body { get; set; } = new List<IASTNode>();
    }

    public class FunctionNode : IASTNode
    {
        public string Type => "Function";
        public int Line { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public DataType ReturnType { get; set; }
        public List<IASTNode> Body { get; set; } = new List<IASTNode>();
    }

    public class Parameter
    {
        public string Name { get; set; } = string.Empty;
        public DataType Type { get; set; }
        public bool ByRef { get; set; }
        public DataType? ArrayElementType { get; set; }
    }

    public class CallNode : IASTNode
    {
        public string Type => "Call";
        public int Line { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<IExpressionNode> Arguments { get; set; } = new List<IExpressionNode>();
    }

    public class ReturnNode : IASTNode
    {
        public string Type => "Return";
        public int Line { get; set; }
        public IExpressionNode Value { get; set; } = null!;
    }

    public class OpenFileNode : IASTNode
    {
        public string Type => "OpenFile";
        public int Line { get; set; }
        public IExpressionNode Filename { get; set; } = null!;
        public FileMode Mode { get; set; }
    }

    public class CloseFileNode : IASTNode
    {
        public string Type => "CloseFile";
        public int Line { get; set; }
        public IExpressionNode Filename { get; set; } = null!;
    }

    public class ReadFileNode : IASTNode
    {
        public string Type => "ReadFile";
        public int Line { get; set; }
        public IExpressionNode Filename { get; set; } = null!;
        public IExpressionNode Target { get; set; } = null!;
    }

    public class WriteFileNode : IASTNode
    {
        public string Type => "WriteFile";
        public int Line { get; set; }
        public IExpressionNode Filename { get; set; } = null!;
        public IExpressionNode Data { get; set; } = null!;
    }

    // Expression Nodes

    public class BinaryOpNode : IExpressionNode
    {
        public string Type => "BinaryOp";
        public int Line { get; set; }
        public string Operator { get; set; } = string.Empty;
        public IExpressionNode Left { get; set; } = null!;
        public IExpressionNode Right { get; set; } = null!;
    }

    public class UnaryOpNode : IExpressionNode
    {
        public string Type => "UnaryOp";
        public int Line { get; set; }
        public string Operator { get; set; } = string.Empty;
        public IExpressionNode Operand { get; set; } = null!;
    }

    public class LiteralNode : IExpressionNode
    {
        public string Type => "Literal";
        public int Line { get; set; }
        public object? Value { get; set; }
        public DataType DataType { get; set; }
    }

    public class IdentifierNode : IExpressionNode
    {
        public string Type => "Identifier";
        public int Line { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ArrayAccessNode : IExpressionNode
    {
        public string Type => "ArrayAccess";
        public int Line { get; set; }
        public string Array { get; set; } = string.Empty;
        public List<IExpressionNode> Indices { get; set; } = new List<IExpressionNode>();
    }

    public class FunctionCallNode : IExpressionNode
    {
        public string Type => "FunctionCall";
        public int Line { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<IExpressionNode> Arguments { get; set; } = new List<IExpressionNode>();
    }

    // Runtime types

    public class Variable
    {
        public DataType Type { get; set; }
        public object? Value { get; set; }
        public List<ArrayDimension>? Dimensions { get; set; }
        public DataType? ElementType { get; set; }
        public bool Initialized { get; set; }
    }

    public class ExecutionContext
    {
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<string, ProcedureNode> Procedures { get; set; } = new Dictionary<string, ProcedureNode>();
        public Dictionary<string, FunctionNode> Functions { get; set; } = new Dictionary<string, FunctionNode>();
        public ExecutionContext? Parent { get; set; }
    }

    public class RuntimeError : Exception
    {
        public int Line { get; }

        public RuntimeError(string message, int line) : base(message)
        {
            Line = line;
        }
    }

    // Debug types

    public enum DebugAction
    {
        Step,
        Continue,
        Stop
    }

    public class CallStackFrame
    {
        public string Name { get; set; } = string.Empty;
        public int Line { get; set; }
        public CallStackType Type { get; set; }
    }

    public enum CallStackType
    {
        Procedure,
        Function,
        Main
    }

    public class DebugState
    {
        public int CurrentLine { get; set; }
        public List<CallStackFrame> CallStack { get; set; } = new List<CallStackFrame>();
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public bool IsPaused { get; set; }
        public bool IsRunning { get; set; }
    }

    public class DebuggerYield
    {
        public string Type { get; set; } = string.Empty; // "output" or "pause"
        public string? Value { get; set; }
        public DebugState? DebugState { get; set; }
    }

    // Execution result

    public class ExecutionResult
    {
        public bool Success { get; set; }
        public List<string> Output { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
        public int? ErrorLine { get; set; }
    }

    public enum ExecutionMode
    {
        Run,
        Debug
    }
}

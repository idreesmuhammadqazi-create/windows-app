namespace PseudoRun.Desktop.Validator
{
    public enum ErrorType
    {
        Syntax,
        Runtime
    }

    public class ValidationError
    {
        public int Line { get; set; }
        public string Message { get; set; } = string.Empty;
        public ErrorType Type { get; set; }
    }
}

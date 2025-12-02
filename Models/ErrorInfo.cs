namespace PseudoRun.Desktop.Models
{
    public class ErrorInfo
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "Error";
        public int Line { get; set; } = 0;
        public int Column { get; set; } = 0;
        public bool HasLocation => Line > 0;
    }
}

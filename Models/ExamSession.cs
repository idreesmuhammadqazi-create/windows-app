using System;

namespace PseudoRun.Desktop.Models
{
    public class ExamSession
    {
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string CodeWritten { get; set; } = string.Empty;
        public bool CompletedSuccessfully { get; set; }
    }
}

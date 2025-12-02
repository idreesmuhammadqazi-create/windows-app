using System.Collections.Generic;

namespace PseudoRun.Desktop.Models
{
    public class Example
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public string? ExamRelevance { get; set; }
        public string Code { get; set; } = string.Empty;
        public List<string>? Tags { get; set; }
    }
}

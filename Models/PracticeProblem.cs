using System.Collections.Generic;

namespace PseudoRun.Desktop.Models
{
    public class PracticeProblem
    {
        public int Id { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Hints { get; set; } = new();
        public string Solution { get; set; } = string.Empty;
    }
}

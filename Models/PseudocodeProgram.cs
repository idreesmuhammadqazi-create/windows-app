using System;

namespace PseudoRun.Desktop.Models
{
    public class PseudocodeProgram
    {
        public string Name { get; set; } = "Untitled";
        public string Code { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }
}

using System.Collections.Generic;

namespace PseudoRun.Desktop.Models
{
    public class SyntaxCategory
    {
        public string Category { get; set; } = string.Empty;
        public List<SyntaxItem> Items { get; set; } = new();
    }
}

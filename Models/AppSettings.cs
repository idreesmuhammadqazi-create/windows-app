using System.Collections.Generic;

namespace PseudoRun.Desktop.Models
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light"; // "Light" or "Dark"
        public int EditorFontSize { get; set; } = 14;
        public bool ShowLineNumbers { get; set; } = true;
        public List<string> RecentFiles { get; set; } = new List<string>();
        public WindowSettings Window { get; set; } = new WindowSettings();
        public int OutputAnimationDelay { get; set; } = 300; // milliseconds
    }

    public class WindowSettings
    {
        public double Width { get; set; } = 1200;
        public double Height { get; set; } = 800;
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public bool IsMaximized { get; set; } = false;
    }
}

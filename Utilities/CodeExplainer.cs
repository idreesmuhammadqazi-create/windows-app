using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PseudoRun.Desktop.Utilities
{
    /// <summary>
    /// Analyzes and explains IGCSE pseudocode structure in plain English
    /// </summary>
    public class CodeExplainer
    {
        public class CodeAnalysis
        {
            public string Summary { get; set; } = string.Empty;
            public List<string> Explanations { get; set; } = new();
            public int ComplexityScore { get; set; }
            public string ComplexityLevel { get; set; } = string.Empty;
            public Dictionary<string, int> Constructs { get; set; } = new();
            public List<string> Suggestions { get; set; } = new();
        }

        /// <summary>
        /// Analyze pseudocode and generate human-readable explanation
        /// </summary>
        public static CodeAnalysis Explain(string code)
        {
            var analysis = new CodeAnalysis();
            var lines = code.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            if (!lines.Any())
            {
                analysis.Summary = "No code to analyze.";
                return analysis;
            }

            // Count constructs
            analysis.Constructs = CountConstructs(lines);

            // Calculate complexity
            analysis.ComplexityScore = CalculateComplexity(analysis.Constructs);
            analysis.ComplexityLevel = GetComplexityLevel(analysis.ComplexityScore);

            // Generate explanations
            analysis.Explanations = GenerateExplanations(lines, analysis.Constructs);

            // Generate summary
            analysis.Summary = GenerateSummary(analysis.Constructs, lines.Count);

            // Generate suggestions
            analysis.Suggestions = GenerateSuggestions(analysis.Constructs, lines);

            return analysis;
        }

        private static Dictionary<string, int> CountConstructs(List<string> lines)
        {
            var constructs = new Dictionary<string, int>
            {
                ["Variables"] = 0,
                ["Input"] = 0,
                ["Output"] = 0,
                ["IfStatements"] = 0,
                ["Loops"] = 0,
                ["Arrays"] = 0,
                ["Procedures"] = 0,
                ["Functions"] = 0,
                ["Comments"] = 0
            };

            foreach (var line in lines)
            {
                var upper = line.ToUpper();

                if (upper.StartsWith("DECLARE"))
                {
                    constructs["Variables"]++;
                    if (line.Contains("[") || line.Contains("ARRAY"))
                        constructs["Arrays"]++;
                }
                if (upper.StartsWith("INPUT"))
                    constructs["Input"]++;
                if (upper.StartsWith("OUTPUT"))
                    constructs["Output"]++;
                if (upper.StartsWith("IF") || upper.Contains("THEN") || upper.Contains("ELSE"))
                    constructs["IfStatements"]++;
                if (upper.StartsWith("FOR") || upper.StartsWith("WHILE") || upper.StartsWith("REPEAT"))
                    constructs["Loops"]++;
                if (upper.StartsWith("PROCEDURE"))
                    constructs["Procedures"]++;
                if (upper.StartsWith("FUNCTION"))
                    constructs["Functions"]++;
                if (upper.StartsWith("//"))
                    constructs["Comments"]++;
            }

            return constructs;
        }

        private static int CalculateComplexity(Dictionary<string, int> constructs)
        {
            int score = 0;
            score += constructs["Variables"];
            score += constructs["Input"] * 2;
            score += constructs["Output"];
            score += constructs["IfStatements"] * 3;
            score += constructs["Loops"] * 5;
            score += constructs["Arrays"] * 4;
            score += constructs["Procedures"] * 6;
            score += constructs["Functions"] * 7;
            return score;
        }

        private static string GetComplexityLevel(int score)
        {
            if (score < 10) return "Beginner";
            if (score < 30) return "Intermediate";
            if (score < 60) return "Advanced";
            return "Expert";
        }

        private static List<string> GenerateExplanations(List<string> lines, Dictionary<string, int> constructs)
        {
            var explanations = new List<string>();

            // Program structure explanation
            if (constructs["Procedures"] > 0 || constructs["Functions"] > 0)
            {
                explanations.Add($"This program uses modular programming with {constructs["Procedures"]} procedure(s) and {constructs["Functions"]} function(s).");
            }

            // Variable usage
            if (constructs["Variables"] > 0)
            {
                explanations.Add($"The program declares {constructs["Variables"]} variable(s) to store data.");
            }

            // Arrays
            if (constructs["Arrays"] > 0)
            {
                explanations.Add($"The program uses {constructs["Arrays"]} array(s) to store multiple values.");
            }

            // Input/Output
            if (constructs["Input"] > 0 && constructs["Output"] > 0)
            {
                explanations.Add($"The program is interactive, using {constructs["Input"]} INPUT statement(s) to get data from the user and {constructs["Output"]} OUTPUT statement(s) to display results.");
            }
            else if (constructs["Output"] > 0)
            {
                explanations.Add($"The program displays information using {constructs["Output"]} OUTPUT statement(s).");
            }

            // Control structures
            if (constructs["IfStatements"] > 0)
            {
                explanations.Add($"The program makes {constructs["IfStatements"]} decision(s) using IF statements to control the flow.");
            }

            if (constructs["Loops"] > 0)
            {
                explanations.Add($"The program uses {constructs["Loops"]} loop(s) to repeat operations.");
            }

            // Code quality
            if (constructs["Comments"] > 0)
            {
                explanations.Add($"Good practice: The code includes {constructs["Comments"]} comment(s) for documentation.");
            }
            else
            {
                explanations.Add("Consider adding comments to explain complex sections of your code.");
            }

            return explanations;
        }

        private static string GenerateSummary(Dictionary<string, int> constructs, int totalLines)
        {
            var sb = new StringBuilder();

            sb.Append($"This is a {GetComplexityLevel(CalculateComplexity(constructs))} level IGCSE pseudocode program ");
            sb.Append($"with {totalLines} line(s) of code. ");

            var features = new List<string>();
            if (constructs["Procedures"] > 0 || constructs["Functions"] > 0)
                features.Add("modular programming");
            if (constructs["Arrays"] > 0)
                features.Add("arrays");
            if (constructs["Loops"] > 0)
                features.Add("iteration");
            if (constructs["IfStatements"] > 0)
                features.Add("selection");
            if (constructs["Input"] > 0)
                features.Add("user input");

            if (features.Any())
            {
                sb.Append("It demonstrates ");
                sb.Append(string.Join(", ", features.Take(features.Count - 1)));
                if (features.Count > 1)
                    sb.Append(" and ");
                sb.Append(features.Last());
                sb.Append(".");
            }

            return sb.ToString();
        }

        private static List<string> GenerateSuggestions(Dictionary<string, int> constructs, List<string> lines)
        {
            var suggestions = new List<string>();

            // Check for common improvements
            if (constructs["Comments"] == 0)
            {
                suggestions.Add("Add comments to explain what your code does, especially for complex sections.");
            }

            if (constructs["Variables"] > 10 && constructs["Procedures"] == 0 && constructs["Functions"] == 0)
            {
                suggestions.Add("Consider breaking down your code into procedures or functions for better organization.");
            }

            if (constructs["Loops"] > 0 && constructs["Arrays"] == 0 && lines.Any(l => l.ToUpper().Contains("REPEAT")))
            {
                suggestions.Add("When processing multiple items, consider using arrays with FOR loops.");
            }

            if (constructs["Input"] > 0 && !lines.Any(l => l.ToUpper().Contains("VALIDATE") || l.ToUpper().Contains("CHECK")))
            {
                suggestions.Add("Consider adding input validation to ensure data is correct before processing.");
            }

            if (constructs["IfStatements"] > 5)
            {
                suggestions.Add("If you have many IF statements, consider using a CASE statement for clearer logic.");
            }

            if (suggestions.Count == 0)
            {
                suggestions.Add("Your code structure looks good! Keep practicing different programming concepts.");
            }

            return suggestions;
        }

        /// <summary>
        /// Generate a formatted explanation report
        /// </summary>
        public static string GenerateReport(CodeAnalysis analysis)
        {
            var sb = new StringBuilder();

            sb.AppendLine("CODE ANALYSIS REPORT");
            sb.AppendLine("===================");
            sb.AppendLine();

            sb.AppendLine("SUMMARY:");
            sb.AppendLine(analysis.Summary);
            sb.AppendLine();

            sb.AppendLine($"COMPLEXITY: {analysis.ComplexityLevel} (Score: {analysis.ComplexityScore})");
            sb.AppendLine();

            sb.AppendLine("CONSTRUCTS USED:");
            foreach (var construct in analysis.Constructs.Where(c => c.Value > 0))
            {
                sb.AppendLine($"  - {construct.Key}: {construct.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("DETAILED EXPLANATION:");
            foreach (var explanation in analysis.Explanations)
            {
                sb.AppendLine($"  • {explanation}");
            }
            sb.AppendLine();

            if (analysis.Suggestions.Any())
            {
                sb.AppendLine("SUGGESTIONS FOR IMPROVEMENT:");
                foreach (var suggestion in analysis.Suggestions)
                {
                    sb.AppendLine($"  💡 {suggestion}");
                }
            }

            return sb.ToString();
        }
    }
}

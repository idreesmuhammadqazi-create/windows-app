using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PseudoRun.Desktop.Utilities
{
    /// <summary>
    /// Detects common IGCSE student mistakes in pseudocode
    /// </summary>
    public class CommonMistakes
    {
        public class Mistake
        {
            public int Line { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Suggestion { get; set; } = string.Empty;
            public string Severity { get; set; } = "Warning"; // Warning, Error, Info
        }

        public class MistakeReport
        {
            public List<Mistake> Mistakes { get; set; } = new();
            public int ErrorCount => Mistakes.Count(m => m.Severity == "Error");
            public int WarningCount => Mistakes.Count(m => m.Severity == "Warning");
            public int InfoCount => Mistakes.Count(m => m.Severity == "Info");
        }

        /// <summary>
        /// Analyze code for common IGCSE student mistakes
        /// </summary>
        public static MistakeReport Analyze(string code)
        {
            var report = new MistakeReport();
            var lines = code.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var lineNumber = i + 1;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check for common mistakes
                CheckAssignmentOperator(line, lineNumber, report);
                CheckVariableDeclaration(line, lineNumber, report);
                CheckArraySyntax(line, lineNumber, report);
                CheckStringComparison(line, lineNumber, report);
                CheckCaseStatementStructure(line, lineNumber, report);
                CheckLoopSyntax(line, lineNumber, report);
                CheckProcedureFunctionCalls(line, lineNumber, report);
                CheckComments(line, lineNumber, report);
                CheckIndentation(lines[i], lineNumber, report);
                CheckKeywordCase(line, lineNumber, report);
            }

            // Multi-line checks
            CheckMissingEndStatements(lines, report);
            CheckUnusedVariables(lines, report);

            return report;
        }

        private static void CheckAssignmentOperator(string line, int lineNumber, MistakeReport report)
        {
            var upper = line.ToUpper();

            // Using = instead of <-
            if (Regex.IsMatch(line, @"^\s*\w+\s*=\s*") && !upper.Contains("IF") && !upper.Contains("WHILE"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Assignment Operator",
                    Message = "Using '=' for assignment instead of '<-'",
                    Suggestion = "In IGCSE pseudocode, use '<-' for assignment. For example: x <- 5",
                    Severity = "Error"
                });
            }

            // Using := instead of <-
            if (line.Contains(":="))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Assignment Operator",
                    Message = "Using ':=' for assignment (Pascal style) instead of '<-'",
                    Suggestion = "IGCSE pseudocode uses '<-' for assignment, not ':='",
                    Severity = "Error"
                });
            }
        }

        private static void CheckVariableDeclaration(string line, int lineNumber, MistakeReport report)
        {
            var upper = line.ToUpper();

            // Missing colon in DECLARE
            if (upper.StartsWith("DECLARE") && !line.Contains(":"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Variable Declaration",
                    Message = "Missing colon in DECLARE statement",
                    Suggestion = "Use: DECLARE variableName : TYPE",
                    Severity = "Error"
                });
            }

            // Using VAR instead of DECLARE
            if (upper.StartsWith("VAR "))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Variable Declaration",
                    Message = "Using 'VAR' instead of 'DECLARE'",
                    Suggestion = "IGCSE pseudocode uses 'DECLARE', not 'VAR'",
                    Severity = "Error"
                });
            }

            // Common type mistakes
            if (upper.Contains("DECLARE") && (upper.Contains("INT ") || upper.Contains("FLOAT")))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Data Type",
                    Message = "Using incorrect data type name",
                    Suggestion = "IGCSE uses INTEGER, REAL, STRING, BOOLEAN, CHAR (not int, float, etc.)",
                    Severity = "Error"
                });
            }
        }

        private static void CheckArraySyntax(string line, int lineNumber, MistakeReport report)
        {
            // Using () instead of [] for arrays
            if (Regex.IsMatch(line, @"\w+\(\d+\)") && !line.ToUpper().Contains("PROCEDURE") && !line.ToUpper().Contains("FUNCTION"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Array Syntax",
                    Message = "Using parentheses () for array access instead of brackets []",
                    Suggestion = "Use square brackets: myArray[0], not myArray(0)",
                    Severity = "Warning"
                });
            }

            // Array declaration without proper syntax
            if (line.ToUpper().Contains("ARRAY") && !line.Contains("[") && !line.Contains("1:") && !line.Contains("0:"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Array Declaration",
                    Message = "Array declaration missing range specification",
                    Suggestion = "Use: DECLARE myArray : ARRAY[1:10] OF INTEGER",
                    Severity = "Info"
                });
            }
        }

        private static void CheckStringComparison(string line, int lineNumber, MistakeReport report)
        {
            // Using == instead of =
            if (line.Contains("=="))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Comparison Operator",
                    Message = "Using '==' for comparison instead of '='",
                    Suggestion = "IGCSE pseudocode uses '=' for comparison, not '=='",
                    Severity = "Error"
                });
            }
        }

        private static void CheckCaseStatementStructure(string line, int lineNumber, MistakeReport report)
        {
            var upper = line.ToUpper();

            // Using SWITCH instead of CASE
            if (upper.StartsWith("SWITCH"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Case Statement",
                    Message = "Using 'SWITCH' instead of 'CASE'",
                    Suggestion = "IGCSE pseudocode uses 'CASE OF', not 'SWITCH'",
                    Severity = "Error"
                });
            }
        }

        private static void CheckLoopSyntax(string line, int lineNumber, MistakeReport report)
        {
            var upper = line.ToUpper();

            // FOR loop without TO
            if (upper.StartsWith("FOR") && !upper.Contains(" TO "))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Loop Syntax",
                    Message = "FOR loop missing 'TO' keyword",
                    Suggestion = "Use: FOR counter <- 1 TO 10",
                    Severity = "Error"
                });
            }

            // WHILE without DO
            if (upper.StartsWith("WHILE") && !upper.Contains(" DO") && !line.TrimEnd().EndsWith("DO"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Loop Syntax",
                    Message = "WHILE loop missing 'DO' keyword",
                    Suggestion = "Use: WHILE condition DO",
                    Severity = "Warning"
                });
            }

            // REPEAT without UNTIL
            if (upper.StartsWith("REPEAT") && lines.Skip(lineNumber).Take(20).All(l => !l.ToUpper().Contains("UNTIL")))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Loop Syntax",
                    Message = "REPEAT loop appears to be missing UNTIL",
                    Suggestion = "Every REPEAT must have a corresponding UNTIL",
                    Severity = "Warning"
                });
            }
        }

        private static void CheckProcedureFunctionCalls(string line, int lineNumber, MistakeReport report)
        {
            var upper = line.ToUpper();

            // CALL before procedure
            if (upper.StartsWith("CALL "))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Procedure Call",
                    Message = "Using 'CALL' keyword before procedure name",
                    Suggestion = "In IGCSE, just write the procedure name directly: MyProcedure()",
                    Severity = "Info"
                });
            }
        }

        private static void CheckComments(string line, int lineNumber, MistakeReport report)
        {
            // Using # or /* */ for comments
            if (line.TrimStart().StartsWith("#") || line.Contains("/*") || line.Contains("*/"))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Comment Syntax",
                    Message = "Using incorrect comment syntax",
                    Suggestion = "IGCSE pseudocode uses // for comments",
                    Severity = "Warning"
                });
            }
        }

        private static void CheckIndentation(string line, int lineNumber, MistakeReport report)
        {
            // Check for mixed tabs and spaces (simplified check)
            if (line.Length > 0 && line[0] == '\t' && line.Contains("    "))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = lineNumber,
                    Type = "Indentation",
                    Message = "Mixed tabs and spaces for indentation",
                    Suggestion = "Use consistent indentation (either tabs or spaces, not both)",
                    Severity = "Info"
                });
            }
        }

        private static void CheckKeywordCase(string line, int lineNumber, MistakeReport report)
        {
            // Keywords in lowercase
            var lowercaseKeywords = new[] { "declare", "if", "then", "else", "endif", "while", "for", "repeat",
                                           "until", "procedure", "function", "return", "input", "output" };

            foreach (var keyword in lowercaseKeywords)
            {
                if (Regex.IsMatch(line, $@"\b{keyword}\b"))
                {
                    report.Mistakes.Add(new Mistake
                    {
                        Line = lineNumber,
                        Type = "Keyword Case",
                        Message = $"Keyword '{keyword}' should be uppercase",
                        Suggestion = $"Use {keyword.ToUpper()} instead of {keyword}",
                        Severity = "Info"
                    });
                    break; // Only report once per line
                }
            }
        }

        private static void CheckMissingEndStatements(string[] lines, MistakeReport report)
        {
            int ifCount = 0, whileCount = 0, forCount = 0, procedureCount = 0, functionCount = 0;
            int endIfCount = 0, endWhileCount = 0, nextCount = 0, endProcedureCount = 0, endFunctionCount = 0;

            foreach (var line in lines)
            {
                var upper = line.Trim().ToUpper();
                if (upper.StartsWith("IF ")) ifCount++;
                if (upper.StartsWith("WHILE ")) whileCount++;
                if (upper.StartsWith("FOR ")) forCount++;
                if (upper.StartsWith("PROCEDURE ")) procedureCount++;
                if (upper.StartsWith("FUNCTION ")) functionCount++;

                if (upper.StartsWith("ENDIF") || upper.StartsWith("END IF")) endIfCount++;
                if (upper.StartsWith("ENDWHILE") || upper.StartsWith("END WHILE")) endWhileCount++;
                if (upper.StartsWith("NEXT") || upper.StartsWith("ENDFOR")) nextCount++;
                if (upper.StartsWith("ENDPROCEDURE") || upper.StartsWith("END PROCEDURE")) endProcedureCount++;
                if (upper.StartsWith("ENDFUNCTION") || upper.StartsWith("END FUNCTION")) endFunctionCount++;
            }

            if (ifCount > endIfCount)
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = 0,
                    Type = "Structure",
                    Message = $"Missing {ifCount - endIfCount} ENDIF statement(s)",
                    Suggestion = "Every IF must have a corresponding ENDIF",
                    Severity = "Error"
                });
            }

            if (whileCount > endWhileCount)
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = 0,
                    Type = "Structure",
                    Message = $"Missing {whileCount - endWhileCount} ENDWHILE statement(s)",
                    Suggestion = "Every WHILE must have a corresponding ENDWHILE",
                    Severity = "Error"
                });
            }

            if (forCount > nextCount)
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = 0,
                    Type = "Structure",
                    Message = $"Missing {forCount - nextCount} NEXT statement(s)",
                    Suggestion = "Every FOR must have a corresponding NEXT",
                    Severity = "Error"
                });
            }
        }

        private static void CheckUnusedVariables(string[] lines, MistakeReport report)
        {
            // This is a simplified check - could be enhanced
            var declaredVars = new HashSet<string>();
            var usedVars = new HashSet<string>();

            foreach (var line in lines)
            {
                var upper = line.ToUpper();
                if (upper.Contains("DECLARE"))
                {
                    var match = Regex.Match(line, @"DECLARE\s+(\w+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        declaredVars.Add(match.Groups[1].Value);
                    }
                }
                else
                {
                    foreach (var variable in declaredVars)
                    {
                        if (line.Contains(variable))
                        {
                            usedVars.Add(variable);
                        }
                    }
                }
            }

            foreach (var unusedVar in declaredVars.Except(usedVars))
            {
                report.Mistakes.Add(new Mistake
                {
                    Line = 0,
                    Type = "Unused Variable",
                    Message = $"Variable '{unusedVar}' is declared but never used",
                    Suggestion = "Remove unused variables or use them in your code",
                    Severity = "Info"
                });
            }
        }

        /// <summary>
        /// Generate a formatted mistake report
        /// </summary>
        public static string GenerateReport(MistakeReport report)
        {
            if (!report.Mistakes.Any())
                return "✅ No common mistakes found! Your code looks good.";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("COMMON MISTAKES REPORT");
            sb.AppendLine("=====================");
            sb.AppendLine();
            sb.AppendLine($"Total Issues: {report.Mistakes.Count}");
            sb.AppendLine($"  Errors: {report.ErrorCount}");
            sb.AppendLine($"  Warnings: {report.WarningCount}");
            sb.AppendLine($"  Info: {report.InfoCount}");
            sb.AppendLine();

            // Group by severity
            foreach (var severity in new[] { "Error", "Warning", "Info" })
            {
                var mistakes = report.Mistakes.Where(m => m.Severity == severity).ToList();
                if (!mistakes.Any()) continue;

                sb.AppendLine($"{severity.ToUpper()}S:");
                foreach (var mistake in mistakes)
                {
                    sb.AppendLine($"  [{mistake.Type}] Line {(mistake.Line > 0 ? mistake.Line.ToString() : "N/A")}");
                    sb.AppendLine($"    ⚠ {mistake.Message}");
                    sb.AppendLine($"    💡 {mistake.Suggestion}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PseudoRun.Desktop.Utilities
{
    /// <summary>
    /// Generates trace tables for pseudocode execution, helping students understand variable changes
    /// </summary>
    public class TraceTableGenerator
    {
        public class TraceTableRow
        {
            public int Line { get; set; }
            public Dictionary<string, string> VariableValues { get; set; } = new();
            public string Statement { get; set; } = string.Empty;
        }

        public class TraceTable
        {
            public List<string> Variables { get; set; } = new();
            public List<TraceTableRow> Rows { get; set; } = new();
        }

        /// <summary>
        /// Generate a trace table from execution history
        /// </summary>
        public static TraceTable Generate(List<TraceTableRow> executionHistory)
        {
            var table = new TraceTable();

            if (executionHistory == null || !executionHistory.Any())
                return table;

            // Collect all unique variables
            var allVariables = new HashSet<string>();
            foreach (var row in executionHistory)
            {
                foreach (var variable in row.VariableValues.Keys)
                {
                    allVariables.Add(variable);
                }
            }
            table.Variables = allVariables.OrderBy(v => v).ToList();

            // Build rows with consistent variable ordering
            foreach (var historyRow in executionHistory)
            {
                var row = new TraceTableRow
                {
                    Line = historyRow.Line,
                    Statement = historyRow.Statement
                };

                foreach (var variable in table.Variables)
                {
                    row.VariableValues[variable] = historyRow.VariableValues.ContainsKey(variable)
                        ? historyRow.VariableValues[variable]
                        : "-";
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Export trace table to CSV format
        /// </summary>
        public static string ExportToCsv(TraceTable table)
        {
            var sb = new StringBuilder();

            // Header row
            sb.Append("Line,Statement");
            foreach (var variable in table.Variables)
            {
                sb.Append($",{variable}");
            }
            sb.AppendLine();

            // Data rows
            foreach (var row in table.Rows)
            {
                sb.Append($"{row.Line},\"{EscapeCsv(row.Statement)}\"");
                foreach (var variable in table.Variables)
                {
                    var value = row.VariableValues.ContainsKey(variable) ? row.VariableValues[variable] : "-";
                    sb.Append($",\"{EscapeCsv(value)}\"");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Export trace table to markdown format for documentation
        /// </summary>
        public static string ExportToMarkdown(TraceTable table)
        {
            if (!table.Variables.Any() || !table.Rows.Any())
                return "No trace data available.";

            var sb = new StringBuilder();

            // Header row
            sb.Append("| Line | Statement |");
            foreach (var variable in table.Variables)
            {
                sb.Append($" {variable} |");
            }
            sb.AppendLine();

            // Separator row
            sb.Append("|------|-----------|");
            foreach (var _ in table.Variables)
            {
                sb.Append("------|");
            }
            sb.AppendLine();

            // Data rows
            foreach (var row in table.Rows)
            {
                sb.Append($"| {row.Line} | {EscapeMarkdown(row.Statement)} |");
                foreach (var variable in table.Variables)
                {
                    var value = row.VariableValues.ContainsKey(variable) ? row.VariableValues[variable] : "-";
                    sb.Append($" {EscapeMarkdown(value)} |");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate a formatted text trace table for display
        /// </summary>
        public static string ExportToText(TraceTable table)
        {
            if (!table.Variables.Any() || !table.Rows.Any())
                return "No trace data available.";

            var sb = new StringBuilder();
            sb.AppendLine("TRACE TABLE");
            sb.AppendLine("===========");
            sb.AppendLine();

            // Calculate column widths
            int lineWidth = 6;
            int stmtWidth = Math.Max(20, table.Rows.Max(r => r.Statement.Length) + 2);
            var varWidths = table.Variables.ToDictionary(
                v => v,
                v => Math.Max(v.Length, table.Rows.Max(r =>
                    r.VariableValues.ContainsKey(v) ? r.VariableValues[v].Length : 1)) + 2
            );

            // Header
            sb.Append($"{"Line".PadRight(lineWidth)} | {"Statement".PadRight(stmtWidth)} |");
            foreach (var variable in table.Variables)
            {
                sb.Append($" {variable.PadRight(varWidths[variable])} |");
            }
            sb.AppendLine();

            // Separator
            sb.Append(new string('-', lineWidth) + "-+-" + new string('-', stmtWidth) + "-+");
            foreach (var variable in table.Variables)
            {
                sb.Append(new string('-', varWidths[variable] + 2) + "+");
            }
            sb.AppendLine();

            // Data rows
            foreach (var row in table.Rows)
            {
                var stmt = row.Statement.Length > stmtWidth - 2
                    ? row.Statement.Substring(0, stmtWidth - 5) + "..."
                    : row.Statement;

                sb.Append($"{row.Line.ToString().PadRight(lineWidth)} | {stmt.PadRight(stmtWidth)} |");
                foreach (var variable in table.Variables)
                {
                    var value = row.VariableValues.ContainsKey(variable) ? row.VariableValues[variable] : "-";
                    if (value.Length > varWidths[variable])
                        value = value.Substring(0, varWidths[variable] - 3) + "...";
                    sb.Append($" {value.PadRight(varWidths[variable])} |");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Replace("\"", "\"\"");
        }

        private static string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return text.Replace("|", "\\|").Replace("\n", " ");
        }
    }
}

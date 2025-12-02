using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace PseudoRun.Desktop.Views.Controls
{
    public partial class EditorControl : UserControl
    {
        private CompletionWindow? _completionWindow;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditorControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public EditorControl()
        {
            InitializeComponent();

            // Set up syntax highlighting
            SetupSyntaxHighlighting();

            // Set up autocomplete
            TextEditor.TextArea.TextEntering += TextArea_TextEntering;
            TextEditor.TextArea.TextEntered += TextArea_TextEntered;

            // Bind text changes
            TextEditor.TextChanged += (s, e) => Text = TextEditor.Text;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EditorControl)d;
            if (control.TextEditor.Text != (string)e.NewValue)
            {
                control.TextEditor.Text = (string)e.NewValue ?? string.Empty;
            }
        }

        private void SetupSyntaxHighlighting()
        {
            // Create custom syntax highlighting for IGCSE pseudocode
            var highlighting = new HighlightingDefinition();

            // Keywords
            var keywordColor = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.Blue),
                FontWeight = FontWeights.Bold
            };

            var keywordRule = new HighlightingRule
            {
                Color = keywordColor,
                Regex = new System.Text.RegularExpressions.Regex(
                    @"\b(DECLARE|CONSTANT|IF|THEN|ELSE|ENDIF|WHILE|DO|ENDWHILE|REPEAT|UNTIL|FOR|TO|STEP|NEXT|" +
                    @"CASE|OF|OTHERWISE|ENDCASE|INTEGER|REAL|STRING|CHAR|BOOLEAN|ARRAY|INPUT|OUTPUT|" +
                    @"PROCEDURE|ENDPROCEDURE|FUNCTION|ENDFUNCTION|RETURN|RETURNS|CALL|BYVAL|BYREF|" +
                    @"TRUE|FALSE|AND|OR|NOT|DIV|MOD|OPENFILE|CLOSEFILE|READFILE|WRITEFILE|EOF|READ|WRITE|APPEND)\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            };

            // Strings
            var stringColor = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(163, 21, 21))
            };

            var stringRule = new HighlightingRule
            {
                Color = stringColor,
                Regex = new System.Text.RegularExpressions.Regex(@"""[^""]*""")
            };

            // Numbers
            var numberColor = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(0, 128, 128))
            };

            var numberRule = new HighlightingRule
            {
                Color = numberColor,
                Regex = new System.Text.RegularExpressions.Regex(@"\b\d+\.?\d*\b")
            };

            // Comments
            var commentColor = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(System.Windows.Media.Colors.Green)
            };

            var commentRule = new HighlightingRule
            {
                Color = commentColor,
                Regex = new System.Text.RegularExpressions.Regex(@"//.*$", System.Text.RegularExpressions.RegexOptions.Multiline)
            };

            var mainRuleSet = new HighlightingRuleSet();
            mainRuleSet.Rules.Add(keywordRule);
            mainRuleSet.Rules.Add(stringRule);
            mainRuleSet.Rules.Add(numberRule);
            mainRuleSet.Rules.Add(commentRule);

            highlighting.MainRuleSet = mainRuleSet;

            TextEditor.SyntaxHighlighting = highlighting;
        }

        private void TextArea_TextEntering(object? sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void TextArea_TextEntered(object? sender, TextCompositionEventArgs e)
        {
            // Show autocomplete on Ctrl+Space or after typing
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Text == " ")
            {
                ShowCompletionWindow();
            }
        }

        private void ShowCompletionWindow()
        {
            if (_completionWindow != null)
                return;

            _completionWindow = new CompletionWindow(TextEditor.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;

            // Add all IGCSE pseudocode keywords and functions
            var completions = new[]
            {
                // Keywords
                "DECLARE", "CONSTANT", "IF", "THEN", "ELSE", "ENDIF",
                "WHILE", "DO", "ENDWHILE", "REPEAT", "UNTIL",
                "FOR", "TO", "STEP", "NEXT",
                "CASE", "OF", "OTHERWISE", "ENDCASE",
                "PROCEDURE", "ENDPROCEDURE", "FUNCTION", "ENDFUNCTION", "RETURN", "RETURNS",
                "CALL", "BYVAL", "BYREF",
                "INPUT", "OUTPUT",
                "AND", "OR", "NOT", "DIV", "MOD",
                "OPENFILE", "CLOSEFILE", "READFILE", "WRITEFILE", "EOF", "READ", "WRITE", "APPEND",
                // Data types
                "INTEGER", "REAL", "STRING", "CHAR", "BOOLEAN", "ARRAY",
                // Literals
                "TRUE", "FALSE",
                // Built-in functions
                "LENGTH", "SUBSTRING", "UCASE", "LCASE",
                "LEFT", "RIGHT", "MID",
                "CHAR_TO_CODE", "CODE_TO_CHAR",
                "INT", "REAL", "STRING",
                "ROUND", "RANDOM"
            };

            foreach (var completion in completions)
            {
                data.Add(new CompletionData(completion));
            }

            _completionWindow.Show();
            _completionWindow.Closed += delegate
            {
                _completionWindow = null;
            };
        }
    }

    public class CompletionData : ICompletionData
    {
        public CompletionData(string text)
        {
            Text = text;
        }

        public System.Windows.Media.ImageSource? Image => null;

        public string Text { get; }

        public object Content => Text;

        public object Description => $"IGCSE Pseudocode keyword: {Text}";

        public double Priority => 1;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}

using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class InputDialog : Window
    {
        public string InputValue { get; private set; } = string.Empty;

        public InputDialog(string variableName, string variableType)
        {
            InitializeComponent();

            PromptTextBlock.Text = $"Enter value for '{variableName}' (Type: {variableType}):";
            InputTextBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            InputValue = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static string? Show(Window owner, string variableName, string variableType)
        {
            var dialog = new InputDialog(variableName, variableType)
            {
                Owner = owner
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.InputValue;
            }

            return null;
        }
    }
}

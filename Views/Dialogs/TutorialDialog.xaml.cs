using PseudoRun.Desktop.ViewModels;
using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class TutorialDialog : Window
    {
        public TutorialViewModel ViewModel { get; }

        public string? SelectedCode { get; private set; }

        public TutorialDialog()
        {
            InitializeComponent();

            ViewModel = new TutorialViewModel();
            DataContext = ViewModel;

            // Subscribe to events
            ViewModel.TryCodeRequested += (sender, code) =>
            {
                var result = MessageBox.Show(
                    "Load this example? This will replace your current code.",
                    "Try This Code",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedCode = code;
                    DialogResult = true;
                    Close();
                }
            };

            ViewModel.FinishRequested += (sender, e) =>
            {
                Close();
            };
        }

        public static string? Show(Window owner)
        {
            var dialog = new TutorialDialog
            {
                Owner = owner
            };

            var result = dialog.ShowDialog();
            return result == true ? dialog.SelectedCode : null;
        }
    }
}

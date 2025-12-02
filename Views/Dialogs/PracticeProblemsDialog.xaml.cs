using PseudoRun.Desktop.ViewModels;
using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class PracticeProblemsDialog : Window
    {
        public PracticeProblemsViewModel ViewModel { get; }

        public string? SelectedSolution { get; private set; }

        public PracticeProblemsDialog()
        {
            InitializeComponent();

            ViewModel = new PracticeProblemsViewModel();
            DataContext = ViewModel;

            // Subscribe to LoadToEditor event
            ViewModel.LoadToEditorRequested += (sender, solution) =>
            {
                SelectedSolution = solution;
                DialogResult = true;
                Close();
            };
        }

        public static string? Show(Window owner)
        {
            var dialog = new PracticeProblemsDialog
            {
                Owner = owner
            };

            var result = dialog.ShowDialog();
            return result == true ? dialog.SelectedSolution : null;
        }
    }
}

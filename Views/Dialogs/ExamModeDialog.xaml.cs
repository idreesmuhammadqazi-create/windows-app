using PseudoRun.Desktop.ViewModels;
using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class ExamModeDialog : Window
    {
        public ExamModeViewModel ViewModel { get; }

        public bool ExamWasStarted { get; private set; }

        public ExamModeDialog()
        {
            InitializeComponent();

            ViewModel = new ExamModeViewModel();
            DataContext = ViewModel;

            // Subscribe to events
            ViewModel.ExamStarted += (sender, e) =>
            {
                ExamWasStarted = true;
                // Disable main window controls (handled by MainWindow)
            };

            ViewModel.ExamCompleted += (sender, e) =>
            {
                // Can close the dialog after completion screen
            };

            ViewModel.ExamCancelled += (sender, e) =>
            {
                DialogResult = false;
                Close();
            };

            // Handle window closing
            Closing += (sender, e) =>
            {
                // If timer is running, confirm exit
                if (ViewModel.IsTimerVisible && !ViewModel.IsPaused)
                {
                    var result = MessageBox.Show(
                        "Exam is still in progress. Exit anyway?",
                        "Exit Exam Mode",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        public static bool Show(Window owner)
        {
            var dialog = new ExamModeDialog
            {
                Owner = owner
            };

            var result = dialog.ShowDialog();
            return dialog.ExamWasStarted;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PseudoRun.Desktop.Services;
using PseudoRun.Desktop.ViewModels;
using PseudoRun.Desktop.Views.Dialogs;
using System;
using System.Windows;

namespace PseudoRun.Desktop.Views
{
    public partial class MainWindow : Window
    {
        private readonly IExportService _exportService;

        public MainWindow()
        {
            InitializeComponent();

            // Get services from DI
            DataContext = App.GetService<MainViewModel>();
            _exportService = App.GetService<IExportService>();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PseudoRun Desktop\nIGCSE Pseudocode Editor and Simulator\n\nVersion 1.0.0",
                "About PseudoRun", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Tutorial_Click(object sender, RoutedEventArgs e)
        {
            var code = TutorialDialog.Show(this);
            if (code != null && DataContext is MainViewModel viewModel)
            {
                viewModel.Code = code;
            }
        }

        private void PracticeProblems_Click(object sender, RoutedEventArgs e)
        {
            var solution = PracticeProblemsDialog.Show(this);
            if (solution != null && DataContext is MainViewModel viewModel)
            {
                viewModel.Code = solution;
            }
        }

        private void SyntaxReference_Click(object sender, RoutedEventArgs e)
        {
            SyntaxReferenceDialog.Show(this);
        }

        private void ExamMode_Click(object sender, RoutedEventArgs e)
        {
            var examStarted = ExamModeDialog.Show(this);
            // If exam was started, could disable certain menu items here
            // For now, just showing the dialog is sufficient
        }

        private async void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                MessageBox.Show("No code to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = "pseudocode.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportToPdfAsync(viewModel.Code, saveDialog.FileName);
                    MessageBox.Show($"Code exported successfully to:\n{saveDialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExportToDocx_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
                return;

            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                MessageBox.Show("No code to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                DefaultExt = ".docx",
                FileName = "pseudocode.docx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportToDocxAsync(viewModel.Code, saveDialog.FileName);
                    MessageBox.Show($"Code exported successfully to:\n{saveDialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

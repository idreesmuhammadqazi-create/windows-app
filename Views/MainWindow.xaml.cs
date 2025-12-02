using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PseudoRun.Desktop.Services;
using PseudoRun.Desktop.ViewModels;
using PseudoRun.Desktop.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PseudoRun.Desktop.Views
{
    public partial class MainWindow : Window
    {
        private readonly IExportService _exportService;
        private readonly IFileService _fileService;

        public MainWindow()
        {
            InitializeComponent();

            // Get services from DI
            DataContext = App.GetService<MainViewModel>();
            _exportService = App.GetService<IExportService>();
            _fileService = App.GetService<IFileService>();

            // Load recent files menu
            Loaded += (s, e) => LoadRecentFilesMenu();
        }

        private void LoadRecentFilesMenu()
        {
            var recentFiles = _fileService.GetRecentFiles();

            // Clear existing items
            RecentFilesMenu.Items.Clear();

            if (recentFiles.Count == 0)
            {
                var noFilesItem = new MenuItem
                {
                    Header = "(No recent files)",
                    IsEnabled = false
                };
                RecentFilesMenu.Items.Add(noFilesItem);
            }
            else
            {
                foreach (var filePath in recentFiles)
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var menuItem = new MenuItem
                    {
                        Header = fileName,
                        ToolTip = filePath
                    };
                    menuItem.Click += async (s, e) => await OpenRecentFile(filePath);
                    RecentFilesMenu.Items.Add(menuItem);
                }

                // Add separator and clear item
                RecentFilesMenu.Items.Add(new Separator());
                var clearItem = new MenuItem
                {
                    Header = "Clear Recent Files"
                };
                clearItem.Click += (s, e) =>
                {
                    _fileService.ClearRecentFiles();
                    LoadRecentFilesMenu();
                };
                RecentFilesMenu.Items.Add(clearItem);
            }
        }

        private async Task OpenRecentFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show($"File not found:\n{filePath}\n\nIt may have been moved or deleted.",
                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoadRecentFilesMenu(); // Refresh menu
                return;
            }

            if (DataContext is MainViewModel viewModel)
            {
                try
                {
                    var content = await _fileService.LoadProgramAsync(filePath);
                    if (content != null)
                    {
                        viewModel.Code = content;
                        viewModel.CurrentFilePath = filePath;
                        LoadRecentFilesMenu(); // Refresh menu to move this file to top
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void Examples_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = App.GetService<ExamplesViewModel>();
            if (viewModel == null) return;

            var dialog = new ExamplesDialog(viewModel)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.LoadedCode))
            {
                if (DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.Code = dialog.LoadedCode;
                }
            }
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

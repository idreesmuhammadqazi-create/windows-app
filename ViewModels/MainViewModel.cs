using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Services;
using System;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ISettingsService _settingsService;
        private readonly IInterpreterService _interpreterService;
        private readonly IValidationService _validationService;
        private readonly DispatcherTimer _validationTimer;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _output = string.Empty;

        [ObservableProperty]
        private string _currentFilePath = string.Empty;

        [ObservableProperty]
        private bool _isRunning = false;

        [ObservableProperty]
        private int _currentLine = 1;

        [ObservableProperty]
        private int _currentColumn = 1;

        [ObservableProperty]
        private ErrorViewModel _errorViewModel = new();

        [ObservableProperty]
        private DebugViewModel _debugViewModel = new();

        public MainViewModel(
            IFileService fileService,
            ISettingsService settingsService,
            IInterpreterService interpreterService,
            IValidationService validationService)
        {
            _fileService = fileService;
            _settingsService = settingsService;
            _interpreterService = interpreterService;
            _validationService = validationService;

            // Set up real-time validation with debouncing (500ms delay)
            _validationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _validationTimer.Tick += async (s, e) =>
            {
                _validationTimer.Stop();
                await ValidateCodeAsync();
            };
        }

        partial void OnCodeChanged(string value)
        {
            // Restart the debounce timer when code changes
            _validationTimer.Stop();
            _validationTimer.Start();
        }

        private async Task ValidateCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                ErrorViewModel.ClearErrorsCommand.Execute(null);
                return;
            }

            try
            {
                var errors = await _validationService.ValidateAsync(Code);

                // Clear previous errors
                ErrorViewModel.ClearErrorsCommand.Execute(null);

                // Add new errors
                foreach (var error in errors)
                {
                    ErrorViewModel.AddError(error.Message, "Syntax Error", error.Line, error.Column);
                }
            }
            catch (Exception)
            {
                // Silently fail validation - don't interrupt user typing
            }
        }

        [RelayCommand]
        private async Task RunProgram()
        {
            if (IsRunning) return;

            IsRunning = true;
            Output = "";
            ErrorViewModel.ClearErrorsCommand.Execute(null);

            try
            {
                // Validate first
                var errors = await _validationService.ValidateAsync(Code);
                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ErrorViewModel.AddError(error.Message, "Syntax Error", error.Line, error.Column);
                        Output += $"Syntax Error (Line {error.Line}, Col {error.Column}): {error.Message}\n";
                    }
                    IsRunning = false;
                    return;
                }

                // Execute
                await foreach (var line in _interpreterService.ExecuteAsync(Code))
                {
                    Output += line + "\n";
                }
            }
            catch (Exception ex)
            {
                ErrorViewModel.AddError(ex.Message, "Runtime Error");
                Output += $"Runtime Error: {ex.Message}\n";
            }
            finally
            {
                IsRunning = false;
            }
        }

        [RelayCommand]
        private void StopProgram()
        {
            _interpreterService.Stop();
            IsRunning = false;
        }

        [RelayCommand]
        private async Task NewFile()
        {
            // TODO: Check for unsaved changes
            Code = "";
            CurrentFilePath = "";
            Output = "";
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Pseudocode File",
                Filter = "Pseudocode Files (*.pseudo)|*.pseudo|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "pseudo"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var content = await _fileService.LoadProgramAsync(dialog.FileName);
                    Code = content ?? string.Empty;
                    CurrentFilePath = dialog.FileName;
                    Output = "";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task SaveFile()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                await SaveFileAs();
                return;
            }

            try
            {
                await _fileService.SaveProgramAsync(CurrentFilePath, Code);
                System.Windows.MessageBox.Show($"File saved successfully:\n{CurrentFilePath}",
                    "Save Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveFileAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Pseudocode File",
                Filter = "Pseudocode Files (*.pseudo)|*.pseudo|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "pseudo",
                FileName = string.IsNullOrEmpty(CurrentFilePath) ? "Program.pseudo" : System.IO.Path.GetFileName(CurrentFilePath)
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _fileService.SaveProgramAsync(dialog.FileName, Code);
                    CurrentFilePath = dialog.FileName;
                    System.Windows.MessageBox.Show($"File saved successfully:\n{dialog.FileName}",
                        "Save Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}

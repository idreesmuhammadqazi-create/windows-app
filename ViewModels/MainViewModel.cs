using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Services;
using System.Windows;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ISettingsService _settingsService;
        private readonly IInterpreterService _interpreterService;
        private readonly IValidationService _validationService;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _output = string.Empty;

        [ObservableProperty]
        private string _currentFilePath = string.Empty;

        [ObservableProperty]
        private bool _isRunning = false;

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
        }

        [RelayCommand]
        private async Task RunProgram()
        {
            if (IsRunning) return;

            IsRunning = true;
            Output = "";

            try
            {
                await foreach (var line in _interpreterService.ExecuteAsync(Code))
                {
                    Output += line + "\n";
                }
            }
            catch (Exception ex)
            {
                Output += $"Error: {ex.Message}\n";
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
            // TODO: Show OpenFileDialog
            // For now, placeholder
        }

        [RelayCommand]
        private async Task SaveFile()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                await SaveFileAs();
                return;
            }

            await _fileService.SaveProgramAsync(CurrentFilePath, Code);
        }

        [RelayCommand]
        private async Task SaveFileAs()
        {
            // TODO: Show SaveFileDialog
            // For now, placeholder
        }
    }
}

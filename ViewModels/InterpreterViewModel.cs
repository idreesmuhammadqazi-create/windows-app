using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class InterpreterViewModel : ViewModelBase
    {
        private readonly IInterpreterService _interpreterService;
        private CancellationTokenSource? _cancellationTokenSource;

        [ObservableProperty]
        private ObservableCollection<string> _outputLines = new();

        [ObservableProperty]
        private bool _isRunning = false;

        [ObservableProperty]
        private bool _isInputRequired = false;

        [ObservableProperty]
        private string _inputPrompt = string.Empty;

        [ObservableProperty]
        private string _userInput = string.Empty;

        public InterpreterViewModel(IInterpreterService interpreterService)
        {
            _interpreterService = interpreterService;
        }

        [RelayCommand]
        private async Task RunProgram(string code)
        {
            if (IsRunning) return;

            IsRunning = true;
            OutputLines.Clear();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await foreach (var line in _interpreterService.ExecuteAsync(code, _cancellationTokenSource.Token))
                {
                    OutputLines.Add(line);

                    // Add delay for animation effect
                    await Task.Delay(300, _cancellationTokenSource.Token);
                }

                OutputLines.Add("Program completed successfully.");
            }
            catch (OperationCanceledException)
            {
                OutputLines.Add("Execution stopped by user.");
            }
            catch (Exception ex)
            {
                OutputLines.Add($"Error: {ex.Message}");
            }
            finally
            {
                IsRunning = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void StopProgram()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
        }

        [RelayCommand]
        private void ClearOutput()
        {
            OutputLines.Clear();
        }

        public void AddOutput(string text)
        {
            OutputLines.Add(text);
        }
    }
}

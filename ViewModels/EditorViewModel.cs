using CommunityToolkit.Mvvm.ComponentModel;
using PseudoRun.Desktop.Services;
using PseudoRun.Desktop.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class EditorViewModel : ViewModelBase
    {
        private readonly IValidationService _validationService;
        private DispatcherTimer _validationTimer;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private List<ValidationError> _errors = new();

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasErrors = false;

        public EditorViewModel(IValidationService validationService)
        {
            _validationService = validationService;

            // Set up debounced validation (500ms)
            _validationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _validationTimer.Tick += async (s, e) =>
            {
                _validationTimer.Stop();
                await ValidateCode();
            };
        }

        partial void OnCodeChanged(string value)
        {
            // Reset timer on each code change
            _validationTimer.Stop();
            _validationTimer.Start();
        }

        private async Task ValidateCode()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                Errors = new List<ValidationError>();
                HasErrors = false;
                ErrorMessage = string.Empty;
                return;
            }

            try
            {
                var errors = await _validationService.ValidateAsync(Code);
                Errors = errors;
                HasErrors = errors.Any();
                ErrorMessage = HasErrors ? string.Join("\n", errors.Select(e => $"Line {e.Line}: {e.Message}")) : string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Validation error: {ex.Message}";
                HasErrors = true;
            }
        }
    }
}

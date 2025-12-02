using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PseudoRun.Desktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class TutorialViewModel : ViewModelBase
    {
        private readonly string _dataFilePath;
        private List<TutorialStep> _allSteps = new();

        [ObservableProperty]
        private int _currentStepIndex = 0;

        [ObservableProperty]
        private TutorialStep? _currentStep;

        [ObservableProperty]
        private bool _canGoBack = false;

        [ObservableProperty]
        private bool _canGoNext = true;

        [ObservableProperty]
        private bool _isLastStep = false;

        [ObservableProperty]
        private string _progressText = string.Empty;

        [ObservableProperty]
        private bool _hasCode = false;

        public event EventHandler<string>? TryCodeRequested;
        public event EventHandler? FinishRequested;

        public TutorialViewModel()
        {
            _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TutorialSteps.json");
            LoadSteps();
        }

        private void LoadSteps()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var steps = JsonConvert.DeserializeObject<List<TutorialStep>>(json);

                    if (steps != null && steps.Any())
                    {
                        _allSteps = steps;
                        UpdateCurrentStep();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tutorial steps: {ex.Message}");
            }
        }

        private void UpdateCurrentStep()
        {
            if (CurrentStepIndex >= 0 && CurrentStepIndex < _allSteps.Count)
            {
                CurrentStep = _allSteps[CurrentStepIndex];
                HasCode = !string.IsNullOrEmpty(CurrentStep.Code);
                CanGoBack = CurrentStepIndex > 0;
                CanGoNext = CurrentStepIndex < _allSteps.Count - 1;
                IsLastStep = CurrentStepIndex == _allSteps.Count - 1;
                ProgressText = $"Step {CurrentStepIndex + 1} of {_allSteps.Count}";
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        private void Next()
        {
            if (IsLastStep)
            {
                FinishRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (CurrentStepIndex < _allSteps.Count - 1)
            {
                CurrentStepIndex++;
                UpdateCurrentStep();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        private void Previous()
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
                UpdateCurrentStep();
            }
        }

        [RelayCommand]
        private void TryCode()
        {
            if (CurrentStep != null && !string.IsNullOrEmpty(CurrentStep.Code))
            {
                TryCodeRequested?.Invoke(this, CurrentStep.Code);
            }
        }
    }
}

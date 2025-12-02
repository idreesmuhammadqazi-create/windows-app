using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Models;
using System;
using System.Collections.ObjectModel;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class ErrorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ErrorInfo> _errors = new();

        [ObservableProperty]
        private bool _hasErrors = false;

        [ObservableProperty]
        private int _errorCount = 0;

        public event EventHandler<ErrorInfo>? JumpToErrorRequested;

        public void AddError(string message, string type = "Error", int line = 0, int column = 0)
        {
            var error = new ErrorInfo
            {
                Message = message,
                Type = type,
                Line = line,
                Column = column
            };

            Errors.Add(error);
            UpdateErrorState();
        }

        public void AddError(ErrorInfo error)
        {
            Errors.Add(error);
            UpdateErrorState();
        }

        [RelayCommand]
        private void ClearErrors()
        {
            Errors.Clear();
            UpdateErrorState();
        }

        [RelayCommand]
        private void JumpToError(ErrorInfo error)
        {
            if (error != null && error.HasLocation)
            {
                JumpToErrorRequested?.Invoke(this, error);
            }
        }

        private void UpdateErrorState()
        {
            ErrorCount = Errors.Count;
            HasErrors = Errors.Count > 0;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class ExamModeViewModel : ViewModelBase
    {
        private DispatcherTimer? _timer;
        private int _totalSeconds;
        private int _remainingSeconds;

        [ObservableProperty]
        private bool _isConfigurationVisible = true;

        [ObservableProperty]
        private bool _isTimerVisible = false;

        [ObservableProperty]
        private bool _isCompletedVisible = false;

        [ObservableProperty]
        private int _selectedDuration = 45;

        [ObservableProperty]
        private string _timeDisplay = "00:00";

        [ObservableProperty]
        private bool _isPaused = false;

        [ObservableProperty]
        private double _progressPercentage = 100.0;

        [ObservableProperty]
        private string _timeColor = "#107C10"; // Green

        public ObservableCollection<int> AvailableDurations { get; } = new()
        {
            15, 30, 45, 60, 90
        };

        public event EventHandler? ExamStarted;
        public event EventHandler? ExamCompleted;
        public event EventHandler? ExamCancelled;

        [RelayCommand]
        private void StartExam()
        {
            _totalSeconds = SelectedDuration * 60;
            _remainingSeconds = _totalSeconds;

            IsConfigurationVisible = false;
            IsTimerVisible = true;

            UpdateTimeDisplay();
            StartTimer();

            ExamStarted?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void PauseResume()
        {
            IsPaused = !IsPaused;

            if (IsPaused)
            {
                _timer?.Stop();
            }
            else
            {
                _timer?.Start();
            }
        }

        [RelayCommand]
        private void EndExam()
        {
            var result = MessageBox.Show(
                "Exit exam mode? Your progress will be saved but the timer will stop.",
                "End Exam",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StopTimer();
                IsTimerVisible = false;
                IsCompletedVisible = true;
                ExamCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            StopTimer();
            ExamCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer = null;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
                UpdateTimeDisplay();

                if (_remainingSeconds == 0)
                {
                    OnTimeExpired();
                }
            }
        }

        private void UpdateTimeDisplay()
        {
            int minutes = _remainingSeconds / 60;
            int seconds = _remainingSeconds % 60;
            TimeDisplay = $"{minutes:D2}:{seconds:D2}";

            // Update progress percentage
            ProgressPercentage = (_remainingSeconds / (double)_totalSeconds) * 100.0;

            // Update color based on remaining time percentage
            if (ProgressPercentage > 50)
            {
                TimeColor = "#107C10"; // Green
            }
            else if (ProgressPercentage > 25)
            {
                TimeColor = "#FF8C00"; // Orange
            }
            else
            {
                TimeColor = "#E81123"; // Red
            }
        }

        private void OnTimeExpired()
        {
            StopTimer();
            IsTimerVisible = false;
            IsCompletedVisible = true;

            // Play notification sound
            SystemSounds.Exclamation.Play();

            // Show notification
            MessageBox.Show(
                "Time's up! Your exam session has ended.",
                "Exam Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ExamCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}

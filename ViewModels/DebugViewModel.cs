using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PseudoRun.Desktop.Interpreter;
using System.Collections.ObjectModel;
using System.Linq;

namespace PseudoRun.Desktop.ViewModels
{
    public partial class DebugViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<VariableDisplay> _variables = new();

        [ObservableProperty]
        private ObservableCollection<CallStackDisplay> _callStack = new();

        [ObservableProperty]
        private int _currentLine = 0;

        [ObservableProperty]
        private bool _isPaused = false;

        [ObservableProperty]
        private bool _isDebugging = false;

        [RelayCommand]
        private void StepOver()
        {
            // Step to next line
            IsPaused = false;
        }

        [RelayCommand]
        private void Continue()
        {
            // Continue execution
            IsPaused = false;
        }

        [RelayCommand]
        private void StopDebug()
        {
            // Stop debugging
            IsDebugging = false;
            IsPaused = false;
            Variables.Clear();
            CallStack.Clear();
        }

        public void UpdateDebugState(DebugState state)
        {
            CurrentLine = state.CurrentLine;
            IsPaused = state.IsPaused;
            IsDebugging = state.IsRunning;

            // Update variables
            Variables.Clear();
            foreach (var kvp in state.Variables)
            {
                Variables.Add(new VariableDisplay
                {
                    Name = kvp.Key,
                    Type = kvp.Value.Type.ToString(),
                    Value = FormatValue(kvp.Value)
                });
            }

            // Update call stack
            CallStack.Clear();
            foreach (var frame in state.CallStack)
            {
                CallStack.Add(new CallStackDisplay
                {
                    Name = frame.Name,
                    Line = frame.Line,
                    Type = frame.Type.ToString()
                });
            }
        }

        private string FormatValue(Variable variable)
        {
            if (!variable.Initialized)
            {
                return "(uninitialized)";
            }

            if (variable.Type == DataType.ARRAY)
            {
                return "[Array]";
            }

            return variable.Value?.ToString() ?? "(null)";
        }
    }

    public class VariableDisplay
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class CallStackDisplay
    {
        public string Name { get; set; } = string.Empty;
        public int Line { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}

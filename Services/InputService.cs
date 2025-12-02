using PseudoRun.Desktop.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PseudoRun.Desktop.Services
{
    public class InputService : IInputService
    {
        public Task<string> GetInputAsync(string variableName, string variableType)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            // Need to run on UI thread
            Application.Current?.Dispatcher.Invoke(() =>
            {
                try
                {
                    var mainWindow = Application.Current.MainWindow;
                    var result = InputDialog.Show(mainWindow, variableName, variableType);

                    if (result != null)
                    {
                        taskCompletionSource.SetResult(result);
                    }
                    else
                    {
                        // User cancelled - return empty string or default value
                        taskCompletionSource.SetResult(string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            return taskCompletionSource.Task;
        }
    }
}

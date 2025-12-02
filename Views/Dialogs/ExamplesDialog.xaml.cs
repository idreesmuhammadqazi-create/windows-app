using PseudoRun.Desktop.ViewModels;
using System;
using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class ExamplesDialog : Window
    {
        private readonly ExamplesViewModel _viewModel;

        public string? LoadedCode { get; private set; }

        public ExamplesDialog(ExamplesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.OnLoadExample = (code) =>
            {
                LoadedCode = code;
                DialogResult = true;
                Close();
            };

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

using PseudoRun.Desktop.ViewModels;
using System.Windows;

namespace PseudoRun.Desktop.Views.Dialogs
{
    public partial class SyntaxReferenceDialog : Window
    {
        public SyntaxReferenceDialog()
        {
            InitializeComponent();

            DataContext = new SyntaxReferenceViewModel();
        }

        public static void Show(Window owner)
        {
            var dialog = new SyntaxReferenceDialog
            {
                Owner = owner
            };

            dialog.ShowDialog();
        }
    }
}

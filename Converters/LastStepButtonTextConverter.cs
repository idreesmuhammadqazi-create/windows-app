using System;
using System.Globalization;
using System.Windows.Data;

namespace PseudoRun.Desktop.Converters
{
    public class LastStepButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLastStep)
            {
                return isLastStep ? "Finish" : "Next";
            }
            return "Next";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

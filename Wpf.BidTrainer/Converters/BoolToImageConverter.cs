using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.BidTrainer.Converters
{
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value
                ? "pack://application:,,,/Resources/correct.png"
                : "pack://application:,,,/Resources/incorrect.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

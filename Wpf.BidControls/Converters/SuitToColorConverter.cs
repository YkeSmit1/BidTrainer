using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Common;

namespace Wpf.BidControls.Converters
{
    public class SuitToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = (Suit)value!;
            return x is Suit.Diamonds or Suit.Hearts ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

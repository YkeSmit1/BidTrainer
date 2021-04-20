using Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Wpf.BidControls.Converters
{
    public class CardToImageRectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bid = (Card)value;
            var suit = bid.Suit;
            var face = bid.Face;
            var topy = suit switch
            {
                Suit.Clubs => 0,
                Suit.Diamonds => 294,
                Suit.Hearts => 196,
                Suit.Spades => 98,
                _ => throw new ArgumentException(nameof(suit)),
            };
            var topx = 73 * (int)face;
            return new Int32Rect(topx, topy, 73, 97);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

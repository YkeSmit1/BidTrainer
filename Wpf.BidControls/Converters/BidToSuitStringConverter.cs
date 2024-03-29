﻿using System;
using System.Globalization;
using System.Windows.Data;
using Common;

namespace Wpf.BidControls.Converters
{
    public class BidToSuitStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bid = (Bid)value;
            return bid!.bidType == BidType.bid ? Util.GetSuitDescription(bid.Suit) : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

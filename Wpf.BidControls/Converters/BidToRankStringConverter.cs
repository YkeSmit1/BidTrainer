﻿using System;
using System.Globalization;
using System.Windows.Data;
using Common;

namespace Wpf.BidControls.Converters
{
    public class BidToRankStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bid = (Bid)value;
            return bid!.bidType == BidType.bid ? bid.Rank : bid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

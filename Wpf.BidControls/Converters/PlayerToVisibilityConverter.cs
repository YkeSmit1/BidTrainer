﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Common;

namespace Wpf.BidControls.Converters
{
    public class PlayerToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Player)value! == Player.South ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

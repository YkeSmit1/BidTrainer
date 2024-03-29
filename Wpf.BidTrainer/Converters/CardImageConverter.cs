﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.BidTrainer.Converters
{
    public class CardImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter!.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value!.Equals(true) ? parameter : DependencyProperty.UnsetValue;
        }
    }
}

﻿using ModelLibrary;
using System;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Helpers
{
    public class LanguageEnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (((Language)value).ToString().Equals((string)parameter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
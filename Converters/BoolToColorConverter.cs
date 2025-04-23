using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ConnectDotsGame.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public IBrush TrueValue { get; set; } = Brushes.Green;
        public IBrush FalseValue { get; set; } = Brushes.Blue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueValue : FalseValue;
            }
            
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
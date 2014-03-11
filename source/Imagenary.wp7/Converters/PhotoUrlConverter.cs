using System;
using System.Globalization;
using System.Windows.Data;
using Imagenary.Core;

namespace Imagenary.Converters
{
    public class PhotoUrlConverter : IValueConverter
    {
        private readonly ImagenarySettings _settings = new ImagenarySettings();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;

            if (string.IsNullOrWhiteSpace(url)) return null;

            return "http://" + _settings.Domain + url;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
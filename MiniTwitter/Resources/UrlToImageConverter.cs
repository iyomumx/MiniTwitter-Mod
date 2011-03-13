using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MiniTwitter.Resources
{
    public class UrlToImageConverter : IValueConverter
    {
        #region IValueConverter メンバ

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string)
            {
                value = new Uri((string)value);
            }

            if (value is Uri)
            {
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.DecodePixelWidth = 48;
                bitmap.DecodePixelHeight = 48;
                bitmap.UriSource = (Uri)value;
                bitmap.EndInit();

                return bitmap;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

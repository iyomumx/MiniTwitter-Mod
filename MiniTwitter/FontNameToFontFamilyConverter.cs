using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace MiniTwitter
{
    public class FontNameToFontFamilyConverter : IValueConverter
    {

        private FontFamilyConverter InternalConverter;

        public FontNameToFontFamilyConverter()
        {
            InternalConverter = new FontFamilyConverter();
        }
        #region IValueConverter 成员

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new FontFamily(string.Format("{0}, Global User Interface, Emoji", value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value ?? string.Empty).ToString();
        }

        #endregion
    }
}

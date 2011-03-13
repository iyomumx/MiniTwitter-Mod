using System;
using System.Windows.Data;

using MiniTwitter.Properties;

namespace MiniTwitter
{
    public class FooterConverter : IValueConverter
    {
        #region IValueConverter メンバ

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (string)value == Settings.Default.TweetFooter && Settings.Default.EnableTweetFooter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace MiniTwitter
{
    class StateToColorConverter : IValueConverter
    {
        #region IValueConverter メンバ

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int state;
            try
            {
                state = (int)value;
            }
            catch
            {
                state = 0;
            }
            if (state == 0) return Brushes.Green;
            if (state == 1) return Brushes.Yellow;
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using MiniTwitter.Net.Twitter;
using MiniTwitter.Properties;

namespace MiniTwitter.Controls
{
    public class TimelineStyleSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string style;
            switch (Settings.Default.TimelineStyle)
            {
                case TimelineStyle.Standard:
                    style = "Standard";
                    break;
                case TimelineStyle.Balloon:
                    style = "Balloon";
                    break;
                case TimelineStyle.List:
                    style = "List";
                    break;
                default:
                    return null;
            }
            return (DataTemplate)Application.Current.FindResource(string.Format("{0}{1}Template", style, item is Status ? (((Status)item).IsReTweeted ? "ReTweet" : "Status") : "Message"));
        }
    }
}

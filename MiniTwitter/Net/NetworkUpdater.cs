using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTwitter.Net
{
    public class NetworkUpdater
    {
        public void CheckForUpdate()
        {
            var client = new WebClient();

            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(WebClient_DownloadStringCompleted);

            client.DownloadStringAsync(new Uri("http://minitwitter.codeplex.com/releases"));
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
            {
                UpdateCheckCompleted(this, new UpdateCheckCompletedEventArgs { Success = false });

                return;
            }

            var match = Regex.Match(e.Result, @"<span id=\""TitleLabel\"" class=\""CodePlexPageHeader\"">(.+?)<\/span>");

            if (match.Success)
            {
                UpdateCheckCompleted(this, new UpdateCheckCompletedEventArgs { Success = true, CurrentVersion = match.Groups[1].Value });
            }
            else
            {
                UpdateCheckCompleted(this, new UpdateCheckCompletedEventArgs { Success = false });
            }
        }

        public event EventHandler<UpdateCheckCompletedEventArgs> UpdateCheckCompleted;
    }

    public class UpdateCheckCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string CurrentVersion { get; set; }
    }
}

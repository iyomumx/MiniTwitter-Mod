using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net
{
    public class StatusEventArgs : EventArgs
    {
        public StatusEventArgs(Twitter.Status status)
        {
            Status = status;
        }

        public Twitter.Status Status { get; private set; }

        public StatusAction Action { get; set; }
    }

    public enum StatusAction
    {
        Update,
        Deleted,
        Favorited,
        Unfavorited,
    }
}

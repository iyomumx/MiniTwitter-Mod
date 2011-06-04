using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net.Twitter
{
    public class UpdateFailedEventArgs : EventArgs
    {
        public UpdateFailedEventArgs(string status, Exception exception = null)
        {
            this.Status = status;
            this.Exception = exception;
        }
        public Exception Exception { get; private set; }
        public string Status { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net.Twitter
{
    public class UpdateFailedEventArgs : EventArgs
    {
        public UpdateFailedEventArgs(string status, ulong? replyID = null, Exception exception = null)
        {
            this.Status = status;
            this.Exception = exception;
            this.In_Reply_To_Status_ID = replyID;
        }
        public Exception Exception { get; private set; }
        public string Status { get; private set; }
        public ulong? In_Reply_To_Status_ID { get; private set; }

    }
}

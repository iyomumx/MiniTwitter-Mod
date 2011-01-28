using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net.Twitter
{
    class UpdateEventArgs : EventArgs
    {
        public UpdateEventArgs(ITwitterItem item)
        {
            Item = item;
        }

        public ITwitterItem Item { get; private set; }
    }
}

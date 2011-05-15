using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net.Twitter
{
    interface ITimeTaged
    {
        DateTime LastModified { get; set; }
        void UpdateChild();
    }
}

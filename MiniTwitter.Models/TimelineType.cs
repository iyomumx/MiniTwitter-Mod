using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter
{
    public enum TimelineType
    {
        Unknown,
        Recent,
        Replies,
        Archive,
        Message,
        User,
        Search,
        List,
        OtherUser,
    }
}

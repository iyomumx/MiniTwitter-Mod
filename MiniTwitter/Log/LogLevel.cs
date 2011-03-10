using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Log
{
    [Flags()]
    public enum LogLevel
    {
        None = 0,
        Action = 1,
        Uri = 2,
        Params = 4,
        Message = 8,
    }
}

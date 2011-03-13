using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter
{
    public enum FilterType
    {
        None,
        Text,
        RegexText,
        Name,
        RegexName,
        Source,
        RegexSource,
    }
}

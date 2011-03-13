using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Net.Twitter
{
    public class List
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string ScreenName { get; set; }

        public static readonly List[] Empty = new List[0];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniTwitter.Themes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class ThemeAttribute : Attribute
    {
        public ThemeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string ThemeDictionaryLocation { get; set; }
    }
}

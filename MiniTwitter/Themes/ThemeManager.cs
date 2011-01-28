using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Reflection;

namespace MiniTwitter.Themes
{
    static class ThemeManager
    {
        /// <summary>
        /// テーマ名とアセンブリ名のディクショナリ
        /// </summary>
        private static readonly Dictionary<string, string> themes = new Dictionary<string, string>();
        private static string currentTheme;

        public static Dictionary<string, string> Themes
        {
            get { return ThemeManager.themes; }
        } 

        public static void LoadFrom(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "MiniTwitter.Themes.*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    var attributes = assembly.GetCustomAttributes(typeof(ThemeAttribute), false);
                    if (attributes.Length == 0)
                    {
                        continue;
                    }
                    foreach (ThemeAttribute attribute in attributes)
                    {
                        themes.Add(attribute.Name, Path.GetFileNameWithoutExtension(file) + ";Component/" + attribute.ThemeDictionaryLocation);
                    }
                }
                catch { }
            }
        }

        public static string GetTheme(int index)
        {
            return Themes.ElementAt(index).Value;
        }

        private static ResourceDictionary LoadTheme(string theme)
        {
            return Application.LoadComponent(new Uri(theme, UriKind.RelativeOrAbsolute)) as ResourceDictionary;
        }

        public static void ApplyTheme(this Application application, string theme)
        {
            if (currentTheme == theme)
            {
                return;
            }
            currentTheme = theme;
            var rd = ThemeManager.LoadTheme(theme);
            if (application.Resources.MergedDictionaries.Count < 3)
            {
                application.Resources.MergedDictionaries.Add(rd);
            }
            else
            {
                application.Resources.MergedDictionaries[2] = rd;
            }
        }
    }
}

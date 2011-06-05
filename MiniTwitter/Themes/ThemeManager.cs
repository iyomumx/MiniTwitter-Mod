/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            Contract.Requires(directory != null, "路径不能为空");
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
            Contract.Requires(index >= 0, "索引必须大于0");
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
            if (application.Resources.MergedDictionaries.Count < 4)
            {
                application.Resources.MergedDictionaries.Add(rd);
            }
            else
            {
                application.Resources.MergedDictionaries[3] = rd;
            }
        }
    }
}

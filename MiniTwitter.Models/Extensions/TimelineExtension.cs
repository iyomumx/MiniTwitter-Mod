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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Extensions
{
    public static class TimelineExtension
    {
        public static void ClearAll(this IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines.ToList())
            {
                Application.Current.Invoke(timeline.Clear);
            }
        }

        public static void RefreshAll(this IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines.ToList())
            {
                Application.Current.Invoke(timeline.View.Refresh);
            }
        }

        public static void ReadAll(this IEnumerable<Timeline> timelines)
        {
            foreach(var timeline in timelines.AsParallel().Where(p => p.Type != TimelineType.Archive))
            {
                timeline.UnreadCount = 0;

                timeline.Items.AsParallel().ForAll(item =>
                {
                    item.IsNewest = false;
                });
            }
        }

        public static void SearchAll(this IEnumerable<Timeline> timelines, string term)
        {
            foreach (var timeline in timelines.ToList())
            {
                timeline.Search(term);
            }
        }

        public static Timeline TypeAt(this IEnumerable<Timeline> timelines, TimelineType type)
        {
            return timelines.SingleOrDefault(p => p.Type == type);
        }

        public static void ForEach(this IEnumerable<Timeline> timelines, Action<Timeline> action)
        {
            foreach (var timeline in timelines.ToList())
            {
                action(timeline);
            }
        }

        public static void Update<T>(this IEnumerable<Timeline> timelines, IEnumerable<T> appendItems) where T : ITwitterItem
        {
            if (appendItems == null)
            {
                return;
            }
            foreach (var timeline in timelines.AsParallel().Where(p => p.Type != TimelineType.Message && p.Type != TimelineType.List && p.Type != TimelineType.Search).ToList())
            {
                Application.Current.Invoke(timeline.Update, appendItems);
            }
        }

        public static void Update<T>(this IEnumerable<Timeline> timelines, TimelineType type, IEnumerable<T> appendItems) where T : ITwitterItem
        {
            if (appendItems == null)
            {
                return;
            }
            foreach(var timeline in timelines.AsParallel().Where(p => p.Type == type).ToList())
            {
                Application.Current.Invoke(timeline.Update, appendItems);
            }
        }

        public static void Remove<T>(this IEnumerable<Timeline> timelines, T removeItem) where T : class, ITwitterItem
        {
            if (removeItem == null)
            {
                return;
            }
            foreach (var timeline in timelines.ToList())
            {
                Application.Current.Invoke(timeline.Remove, removeItem);
            }
        }

        public static void RemoveAll(this IEnumerable<Timeline> timelines, Predicate<ITwitterItem> match)
        {
            foreach (var timeline in timelines.ToList())
            {
                timeline.RemoveAll(match);
            }
        }

        public static T[] Normalize<T>(this IEnumerable<Timeline> timelines, IEnumerable<T> items) where T : ITwitterItem
        {
            return timelines.Normalize(TimelineType.Recent, items);
        }

        public static T[] Normalize<T>(this IEnumerable<Timeline> timelines, TimelineType type, IEnumerable<T> items) where T : ITwitterItem
        {
            return timelines.Single(p => p.Type == type).Normalize(items);
        }

        public static void Sort(this IEnumerable<Timeline> timelines, ListSortCategory category, ListSortDirection direction)
        {
            foreach (var timeline in timelines.ToList())
            {
                timeline.Sort(category, direction);
            }
        }
    }
}

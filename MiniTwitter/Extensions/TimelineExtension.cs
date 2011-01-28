using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Extensions
{
    static class TimelineExtension
    {
        public static void ClearAll(this IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines)
            {
                Application.Current.Invoke(timeline.Clear);
            }
        }

        public static void RefreshAll(this IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines)
            {
                Application.Current.Invoke(timeline.View.Refresh);
            }
        }

        public static void ReadAll(this IEnumerable<Timeline> timelines)
        {
            foreach (var timeline in timelines.Where(p => p.Type != TimelineType.Archive))
            {
                timeline.UnreadCount = 0;

                foreach (var item in timeline.Items)
                {
                    item.IsNewest = false;
                }
            }
        }

        public static void SearchAll(this IEnumerable<Timeline> timelines, string term)
        {
            foreach (var timeline in timelines)
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
            foreach (var timeline in timelines)
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
            foreach (var timeline in timelines.Where(p => p.Type != TimelineType.Message && p.Type != TimelineType.List && p.Type != TimelineType.Search))
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
            foreach (var timeline in timelines.Where(p => p.Type == type))
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
            foreach (var timeline in timelines)
            {
                Application.Current.Invoke(timeline.Remove, removeItem);
            }
        }

        public static void RemoveAll(this IEnumerable<Timeline> timelines, Predicate<ITwitterItem> match)
        {
            foreach (var timeline in timelines)
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
            foreach (var timeline in timelines)
            {
                timeline.Sort(category, direction);
            }
        }
    }
}

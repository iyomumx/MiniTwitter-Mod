using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml.Serialization;

using MiniTwitter.Extensions;
using MiniTwitter.Net.Twitter;

namespace MiniTwitter
{
    [Serializable]
    public class Timeline : PropertyChangedBase
    {
        public Timeline()
        {
            SinceID = 0;
            VerticalOffset = 0;
            UnreadCount = 0;
            MaxCount = 10000;
            Name = "";
            Type = TimelineType.User;
            Items = new ObservableCollection<ITwitterItem>();
            View = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
        }

        private readonly object _thisLock = new object();

        private string _name;

        [XmlAttribute]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        [XmlAttribute]
        public TimelineType Type { get; set; }

        [XmlAttribute]
        public int MaxCount { get; set; }

        [XmlAttribute]
        public string Tag { get; set; }

        private int _unreadCount;

        [XmlIgnore]
        public int UnreadCount
        {
            get { return _unreadCount; }
            set
            {
                if (_unreadCount != value)
                {
                    _unreadCount = value;
                    OnPropertyChanged("UnreadCount");
                }
            }
        }

        private List<Filter> _filters;

        [XmlElement("Filter")]
        public List<Filter> Filters
        {
            get { return _filters ?? (_filters = new List<Filter>()); }
            set { _filters = value; }
        }

        [XmlIgnore]
        public ListCollectionView View { get; private set; }

        [XmlIgnore]
        public ulong SinceID { get; set; }

        [XmlIgnore]
        public double VerticalOffset { get; set; }

        [XmlIgnore]
        public ObservableCollection<ITwitterItem> Items { get; private set; }

        public void Update<T>(IEnumerable<T> appendItems) where T : ITwitterItem
        {
            lock (_thisLock)
            {
                if (appendItems == null)
                {
                    return;
                }
                int count = 0;
                int unreadCount = 0;
                foreach (var item in appendItems.Where(x => !Items.Contains(x) && IsFilterMatch(x)))
                {
                    if (Type == TimelineType.Archive && item.IsReTweeted)
                    {
                        continue;
                    }
                    count++;
                    if (item.IsNewest)
                    {
                        unreadCount++;
                    }
                    Items.Add(item);
                }
                UnreadCount += unreadCount;
                if (Items.Count > MaxCount)
                {
                    var deleteItems = (from p in Items orderby p.CreatedAt select p).Take(Items.Count - MaxCount).TakeWhile(p => !p.IsNewest);
                    foreach (var item in deleteItems)
                    {
                        Items.Remove(item);
                    }
                }
                if (VerticalOffset != 0.0)
                {
                    VerticalOffset += count;
                }
                //View.Refresh();
            }
        }

        public void Remove(ITwitterItem removeItem)
        {
            lock (_thisLock)
            {
                if (removeItem == null)
                {
                    return;
                }
                if (Items.Remove(removeItem))
                {
                    if (removeItem.IsNewest)
                    {
                        UnreadCount--;
                    }
                    //View.Refresh();
                }
            }
        }

        public void RemoveAll(Predicate<ITwitterItem> match)
        {
            lock (_thisLock)
            {
                int count = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (match(Items[i]))
                    {
                        if (Items[i].IsNewest)
                        {
                            count++;
                        }
                        Items.RemoveAt(i--);
                    }
                }
                UnreadCount -= count;

                if (count != 0)
                {
                    //View.Refresh();
                }
            }
        }

        public void Clear()
        {
            lock (_thisLock)
            {
                UnreadCount = 0;
                View.Filter = null;
                Items.Clear();
                //View.Refresh();
            }
        }

        public T[] Normalize<T>(IEnumerable<T> items) where T : ITwitterItem
        {
            lock (_thisLock)
            {
                if (items == null)
                {
                    return null;
                }
                return items.Where(x => !Items.Contains(x) && IsFilterMatch(x)).ToArray();
            }
        }

        public void Sort(ListSortCategory category, ListSortDirection direction)
        {
            lock (_thisLock)
            {
                View.SortDescriptions.Clear();
                View.SortDescriptions.Add(new SortDescription(category.ToPropertyPath(), direction));
                if (category != ListSortCategory.CreatedAt)
                {
                    View.SortDescriptions.Add(new SortDescription(ListSortCategory.CreatedAt.ToPropertyPath(), direction));
                }
                View.SortDescriptions.Add(new SortDescription(ListSortCategory.ID.ToPropertyPath(), direction));
            }
        }

        public void Search(string term)
        {
            lock (_thisLock)
            {
                if (term.IsNullOrEmpty())
                {
                    View.Filter = null;
                }
                else
                {
                    View.Filter = state =>
                        {
                            var item = (ITwitterItem)state;
                            return item.Text.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1 || item.Sender.ScreenName.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1;
                        };
                }
                View.Refresh();
            }
        }

        public void Trim()
        {
        }

        private bool IsFilterMatch(ITwitterItem item)
        {
            return Filters.Count == 0 || Filters.All(filter => filter.Process(item));//TODO:过滤器/筛选器逻辑（All与Any）
        }
    }
}
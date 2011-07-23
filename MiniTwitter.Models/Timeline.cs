/*
 * 此文件由Apache License 2.0授权的文件修改而来
 * 根据协议要求在此声明此文件已被修改
 * 
 * 未被修改的原始文件可以在
 * https://github.com/iyomumx/MiniTwitter-Mod/tree/minitwitter
 * 找到
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            RecentTweetCount = 0;
            VerticalOffset = 0;
            UnreadCount = 0;
            MaxCount = 10000;
            Name = "";
            Type = TimelineType.User;
            Items = new ObservableCollection<ITwitterItem>();
            View = (ListCollectionView)CollectionViewSource.GetDefaultView(Items);
            View.Filter = commonPredicate;
        }

        static Timeline()
        {
            GlobalFilter = new ObservableCollection<Filter>();
        }

        public static ObservableCollection<Filter> GlobalFilter
        {
            get;
            private set;
        }

        public static void SetFilter(ObservableCollection<Filter> filters)
        {
            GlobalFilter = filters?? new ObservableCollection<Filter>();
        }

        public static readonly Predicate<Object> commonPredicate= item =>
            {
                var twitterItem = (ITwitterItem)item;
                return GlobalFilter.Count == 0 || GlobalFilter.AsParallel().All(filter => filter.Process(twitterItem));
            };

        public static bool ThrowFilteredTweets { get; set; }

        [NonSerialized]
        private Object _thisLock = new Object();

        [NonSerialized]
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
                    Interlocked.Exchange(ref _unreadCount, value);
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
        public ulong MaxID
        {
            get
            {
                if (Items.Count == 0)
                {
                    return 1;
                }
                else
                {
                    return Items.AsParallel().Min(itm => itm.ID);
                }
            }
        }

        [XmlIgnore]
        public int RecentTweetCount { get; set; }

        [XmlIgnore]
        public double VerticalOffset { get; set; }

        [XmlIgnore]
        public ObservableCollection<ITwitterItem> Items { get; private set; }

        public void Update<T>(IEnumerable<T> appendItems) where T : ITwitterItem
        {
            lock(_thisLock)
            {
                if (appendItems == null)
                {
                    return;
                }
                int count = 0;
                int unreadCount = 0;
                var ids = from i in Items select i.ID;
                foreach(var item in appendItems.Where(x => !Items.Contains(x) && IsFilterMatch(x)))
                { 
                    if (Type == TimelineType.Archive && item.IsReTweeted)
                    {
                        //continue;
                        //TODO:某处没有修正这个问题，在此临时解决
                        item.IsAuthor = true;
                    }
                    Interlocked.Increment(ref count);
                    if (item.IsNewest)
                    {
                        Interlocked.Increment(ref unreadCount);
                    }
                    Items.Add(item);
                    if (item is Status)
                    {
                        var itm = ((Status)Convert.ChangeType(item, typeof(Status)));
                        if (itm.IsReply)
                        {
                            var target = Items.AsParallel().OfType<Status>().Where((it, _) => it.ID == itm.InReplyToStatusID).FirstOrDefault();
                            if (target==null)
                            {
                                target = appendItems.AsParallel().OfType<Status>().Where((it, _) => it.ID == itm.InReplyToStatusID).FirstOrDefault();
                                if (target==null)
                                {
                                    continue;
                                }
                            }
                            target.IsReplied = true;
                            target.MentionStatus = itm;
                            itm.InReplyToStatus = target;
                        }
                    }
                    
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
            lock(_thisLock)
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
            lock(_thisLock)
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
            lock(_thisLock)
            {
                UnreadCount = 0;
                View.Filter = commonPredicate;
                Items.Clear();
                //View.Refresh();
            }
        }

        public T[] Normalize<T>(IEnumerable<T> items) where T : ITwitterItem
        {
            lock(_thisLock)
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
            lock(_thisLock)
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
            lock(_thisLock)
            {
                if (term.IsNullOrEmpty())
                {
                    View.Filter = commonPredicate;
                }
                else
                {
                    View.Filter = state =>
                        {
                            var item = (ITwitterItem)state;
                            return (item.Text.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1 || item.Sender.ScreenName.IndexOf(term, StringComparison.OrdinalIgnoreCase) != -1) && (commonPredicate(item));
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
            return ((Filters.Count == 0 || Filters.AsParallel().All(filter => filter.Process(item))) && (!ThrowFilteredTweets || commonPredicate(item)));
            //TODO:过滤器/筛选器逻辑（All与Any）
        }
    }
}
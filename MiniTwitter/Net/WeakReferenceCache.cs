using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;

using MiniTwitter.Net.Twitter;

namespace MiniTwitter.Net
{
    public class WeakReferenceCache
    {
        private ConcurrentDictionary<ulong, WeakReference> _cache;

        public WeakReferenceCache()
        {
            _cache = new ConcurrentDictionary<ulong, WeakReference>();
        }

        #region DictionaryMethods

        public bool ContainsKey(ulong key)
        {
            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(ulong key, out Status value)
        {
            WeakReference t;
            var result = _cache.TryGetValue(key, out t);
            if (!result)
            {
                value = null;
                return false;
            }
            if (t.IsAlive)
            {
                value = (Status)t.Target;
                return true;
            }
            else
            {
                t = null;
                _cache.TryRemove(key, out t);
                value = null;
                return false;
            }
        }

        public bool TryAdd(ulong key, Status value)
        {
            var newwr = new WeakReference(value);
            _cache.AddOrUpdate(key, newwr, (k, wr1) => wr1.IsAlive ? wr1 : newwr);
            return true;
        }

        #endregion

        public Dictionary<ulong,Status> GetDictionary()
        {
            var dic = (from kvp in _cache.AsParallel()
                       where kvp.Value.IsAlive
                       select new KeyValuePair<ulong, Status>(kvp.Key,
                           (Status)kvp.Value.Target)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return dic;
        }

        #region DebuggingMethods
        public long AliveCount
        {
            get
            {
                return _cache.AsParallel().Count(i => i.Value.IsAlive);
            }
        }

        public List<KeyValuePair<ulong,WeakReference>> GetDeadItems()
        {
            return _cache.AsParallel().Where(kvp => !kvp.Value.IsAlive).ToList();
        }

        private bool compressing = false;
        private DateTime lastCompress = DateTime.Now;
        private const double compressInterval = 3.0;

        public long CompressCache()
        {
            if (compressing)
            {
                return 0;
            }
            if (DateTime.Now - lastCompress <= TimeSpan.FromMinutes(compressInterval))
            {
                return 0;
            }
            compressing = true;
            long count = 0;
            try
            {
#if DEBUG
                Debug.WriteLine("[Before GC]Cache Count Is {0}, {1} Alive.", _cache.Count, AliveCount);
#endif
                GC.Collect();
                GC.WaitForPendingFinalizers();
#if DEBUG
                Debug.WriteLine("[After  GC]Cache Count Is {0}, {1} Alive.", _cache.Count, AliveCount);
#endif
                WeakReference o;
                foreach (var item in GetDeadItems())
                {
                    _cache.TryRemove(item.Key, out o);
                    count++;
                }
#if DEBUG
                Debug.WriteLine("[After DEL]Cache Count Is {0}, {1} Alive.", _cache.Count, AliveCount);
#endif
            }
            finally
            {
                lastCompress = DateTime.Now;
                compressing = false;
            }
            return count;
        }
        #endregion
    }
}

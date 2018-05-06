using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace CoreDemo.Caching
{
    public class MemoryCacheManager : IStaticCacheManager
    {
        private readonly IMemoryCache _cache;
        
        protected CancellationTokenSource _cancellationTokenSource;
        
        protected static readonly ConcurrentDictionary<string, bool> _allKeys;
        
        static MemoryCacheManager()
        {
            _allKeys = new ConcurrentDictionary<string, bool>();
        }
        
        public MemoryCacheManager(IMemoryCache cache)
        {
            _cache = cache;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        public virtual T Get<T>(string key)
        {
            return _cache.Get<T>(key);
        }

        public virtual T Get<T>(string key, int cacheTime, Func<T> acquire)
        {
            //item already is in cache, so return it
            if (IsSet(key))
                return Get<T>(key);

            //or create it using passed function
            var result = acquire();

            //and set in cache (if cache time is defined)
            if (cacheTime > 0)
                Set(key, result, cacheTime);

            return result;
        }
        
        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data != null)
            {
                _cache.Set(AddKey(key), data, GetMemoryCacheEntryOptions(TimeSpan.FromMinutes(cacheTime)));
            }
        }
        
        public virtual void Remove(string key)
        {
            _cache.Remove(RemoveKey(key));
        }
        
        public virtual void Clear()
        {
            //send cancellation request
            _cancellationTokenSource.Cancel();

            //releases all resources used by this cancellation token
            _cancellationTokenSource.Dispose();

            //recreate cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public bool IsSet(string key)
        {
            return _cache.TryGetValue(key, out object _);
        }

        public virtual void Dispose()
        {
            //nothing special
        }

        protected MemoryCacheEntryOptions GetMemoryCacheEntryOptions(TimeSpan cacheTime)
        {
            var options = new MemoryCacheEntryOptions()
                // add cancellation token for clear cache
                .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token))
                //add post eviction callback
                .RegisterPostEvictionCallback(PostEviction);

            //set cache time
            options.AbsoluteExpirationRelativeToNow = cacheTime;

            return options;
        }

        protected string AddKey(string key)
        {
            _allKeys.TryAdd(key, true);
            return key;
        }

        protected string RemoveKey(string key)
        {
            TryRemoveKey(key);
            return key;
        }

        protected void TryRemoveKey(string key)
        {
            //try to remove key from dictionary
            if (!_allKeys.TryRemove(key, out bool _))
                //if not possible to remove key from dictionary, then try to mark key as not existing in cache
                _allKeys.TryUpdate(key, false, false);
        }

        private void ClearKeys()
        {
            foreach (var key in _allKeys.Where(p => !p.Value).Select(p => p.Key).ToList())
            {
                RemoveKey(key);
            }
        }

        private void PostEviction(object key, object value, EvictionReason reason, object state)
        {
            //if cached item just change, then nothing doing
            if (reason == EvictionReason.Replaced)
                return;

            //try to remove all keys marked as not existing
            ClearKeys();

            //try to remove this key from dictionary
            TryRemoveKey(key.ToString());
        }
    }
}

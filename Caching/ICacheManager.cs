using System;

namespace CoreDemo.Caching
{
    public interface ICacheManager : IDisposable
    {
        T Get<T>(string key);

        T Get<T>(string key, int cacheTime, Func<T> acquire);

        void Set(string key, object data, int cacheTime);

        bool IsSet(string key);

        void Remove(string key);
        
        void Clear();
    }
}

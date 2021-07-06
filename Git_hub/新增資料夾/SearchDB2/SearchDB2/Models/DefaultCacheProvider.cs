using SearchDB2.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace SearchDB2.Models
{
    public class DefaultCacheProvider : ICacheProvider
    {
        public ObjectCache Cache { get { return MemoryCache.Default; } }

        private string _key = Controllers.HomeController.Id;

        public object Get(string key)
        {
            return Cache[key + _key];
        }

        public void Invalidate(string key)
        {
            Cache.Remove(key + _key);
        }

        public bool IsSet(string key)
        {
            return (Cache[key + _key] != null);
        }

        public void Set(string key, object data, int cacheTime = 30)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime);
            Cache.Add(new CacheItem(key + _key, data), policy);
        }
    }
}
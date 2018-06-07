using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using Treasury.Web.Service.Interface;

namespace Treasury.Web.Service.Actual
{
    public class DefaultCacheProvider : ICacheProvider
    {
        public ObjectCache Cache { get { return MemoryCache.Default; } }

        private string _UserId = Controllers.AccountController.CurrentUserId;

        public object Get(string key)
        {
            return Cache[key + _UserId];
        }

        public void Invalidate(string key)
        {
            Cache.Remove(key + _UserId);
        }

        public bool IsSet(string key)
        {
            return (Cache[key + _UserId] != null);
        }

        public void Set(string key, object data, int cacheTime = 30)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime);
            Cache.Add(new CacheItem(key + _UserId, data), policy);
        }
    }
}
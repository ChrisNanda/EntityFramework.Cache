using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFCodeFirstCacheExtensions;
using Microsoft.ApplicationServer.Caching;

namespace AppFabricCacheProvider
{
    public class AppFabricCacheProvider : IEFCacheProvider
    {
        private AppFabricCacheProvider() { }

        private static object locker = new object();
        private static AppFabricCacheProvider instance;
        private static DataCache cache;

        public static AppFabricCacheProvider GetInstance()
        {
            lock (locker)
            {
                if (instance == null)
                {
                    instance = new AppFabricCacheProvider();
                    DataCacheFactory factory = new DataCacheFactory();
                    cache = factory.GetCache("Default");
                }
            }
            return instance;
        }
        public IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query, TimeSpan cacheDuration)
        {
            string key = GetKey<T>(query);

            var cacheItem = cache.Get(key);
            if (cacheItem == null)
            {
                cache.Put(key, query.ToList(), cacheDuration);
                foreach (var oneItem in query)
                {
                    yield return oneItem;
                }
            }
            else
            {
                foreach (var oneItem in ((List<T>)cacheItem))
                {
                    yield return oneItem;
                }
            }
        }

        public IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query)
        {
            string key = GetKey<T>(query);

            var cacheItem = cache.Get(key);
            if (cacheItem == null)
            {
                cache.Put(key, query.ToList());
                foreach (var oneItem in query)
                {
                    yield return oneItem;
                }
            }
            else
            {
                foreach (var oneItem in ((List<T>)cacheItem))
                {
                    yield return oneItem;
                }
            }
        }

        public bool RemoveFromCache<T>(IQueryable<T> query)
        {
            string key = GetKey<T>(query);
            CacheItem item = null;
            return cache.Remove(key);
        }

        private static string GetKey<T>(IQueryable<T> query)
        {
            string key = string.Concat(query.ToString(), "\n\r", 
                typeof(T).AssemblyQualifiedName);
            return key;
        }
    }
}

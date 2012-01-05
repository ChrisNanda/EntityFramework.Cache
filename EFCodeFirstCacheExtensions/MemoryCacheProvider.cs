using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace EFCodeFirstCacheExtensions
{
    public class MemoryCacheProvider : IEFCacheProvider
    {
        private MemoryCacheProvider() { }

        public static MemoryCacheProvider GetInstance()
        {
            lock (locker)
            {
                if (dictionary == null)
                {
                    dictionary = new ConcurrentDictionary<string, CacheItem>();
                }

                if (instance == null)
                {
                    instance = new MemoryCacheProvider();
                }
            }
            return instance;
        }

        private static ConcurrentDictionary<string, CacheItem> dictionary;
        private static MemoryCacheProvider instance;
        private static object locker = new object();

        public IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query, TimeSpan cacheDuration)
        {
            string key = GetKey<T>(query);

            CacheItem item = dictionary.GetOrAdd(
                key,
                (keyToFind) => { return new CacheItem() 
                    { Item = query.ToList(), AdditionTime = DateTime.Now }; });

            if (DateTime.Now.Subtract(item.AdditionTime) > cacheDuration)
            {
                item = dictionary.AddOrUpdate(
                    key,
                    new CacheItem() { Item = item.Item, AdditionTime = DateTime.Now },
                    (keyToFind, oldItem) => { return new CacheItem() 
                        { Item = query.ToList(), AdditionTime = DateTime.Now }; });
            }
            foreach (var oneItem in ((List<T>)item.Item))
            {
                yield return oneItem;
            }
        }

        public IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query)
        {
            string key = GetKey<T>(query);

            CacheItem item = dictionary.GetOrAdd(
                key,
                (keyToFind) => { return new CacheItem() 
                    { Item = query.ToList(), AdditionTime = DateTime.Now }; });

            foreach (var oneItem in ((List<T>)item.Item))
            {
                yield return oneItem;
            }
        }

        public bool RemoveFromCache<T>(IQueryable<T> query)
        {
            string key = GetKey<T>(query);
            CacheItem item = null;
            return dictionary.TryRemove(key, out item);
        }

		public static string GetKey<T>(IQueryable<T> query)
		{
			var keyBuilder = new StringBuilder(query.ToString());
			var queryParamVisitor = new QueryParameterVisitor(keyBuilder);
			queryParamVisitor.GetQueryParameters(query.Expression);
			keyBuilder.Append("\n\r");
			keyBuilder.Append(typeof(T).AssemblyQualifiedName);

			return keyBuilder.ToString();
		}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCodeFirstCacheExtensions
{
    public static class EFCacheExtensions
    {
        public static void SetCacheProvider(IEFCacheProvider provider)
        {
            cacheProvider = provider;
        }

        private static IEFCacheProvider cacheProvider;
        public static IEnumerable<T> AsCacheable<T>(this IQueryable<T> query)
        {
            if (cacheProvider == null)
            {
                throw new InvalidOperationException("Please set cache provider (call SetCacheProvider) before using caching");
            }
            return cacheProvider.GetOrCreateCache<T>(query);
        }

        public static IEnumerable<T> AsCacheable<T>(this IQueryable<T> query, TimeSpan cacheDuration)
        {
            if (cacheProvider == null)
            {
                throw new InvalidOperationException("Please set cache provider (call SetCacheProvider) before using caching");
            }
            return cacheProvider.GetOrCreateCache<T>(query, cacheDuration);
        }

        public static bool RemoveFromCache<T>(IQueryable<T> query)
        {
            if (cacheProvider == null)
            {
                throw new InvalidOperationException("Please set cache provider (call SetCacheProvider) before using caching");
            }
            return cacheProvider.RemoveFromCache<T>(query);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCodeFirstCacheExtensions
{
    public interface IEFCacheProvider
    {
        IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query);
        IEnumerable<T> GetOrCreateCache<T>(IQueryable<T> query, TimeSpan cacheDuration);
        bool RemoveFromCache<T>(IQueryable<T> query);
    }
}

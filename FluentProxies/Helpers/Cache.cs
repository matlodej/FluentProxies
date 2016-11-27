using FluentProxies.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Helpers
{
    internal static class Cache
    {
        private static Cache<object, object> _wrapperCache = new Cache<object, object>();

        private static Cache<ProxyBlueprint, Type> _typeCache = new Cache<ProxyBlueprint, Type>();

        internal static Cache<object, object> Wrappers
        {
            get
            {
                return _wrapperCache;
            }
        }

        internal static Cache<ProxyBlueprint, Type> Types
        {
            get
            {
                return _typeCache;
            }
        }
    }

    internal class Cache<TKey, TValue>
        where TValue : class
    {
        private ConcurrentDictionary<TKey, TValue> _cache = new ConcurrentDictionary<TKey, TValue>();

        internal virtual bool TryAdd(TKey key, TValue value)
        {
            return _cache.TryAdd(key, value);
        }

        internal virtual bool TryGet(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        internal virtual List<TValue> GetAll()
        {
            return _cache
                .Select(x => x.Value)
                .ToList();
        }

        internal virtual List<TValue> GetAll(TKey key, Func<TKey, TKey, bool> comparer)
        {
            return _cache
                .Where(x => comparer(x.Key, key))
                .Select(x => x.Value)
                .ToList();
        }
    }
}

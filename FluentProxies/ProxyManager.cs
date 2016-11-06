using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction;

namespace FluentProxies
{
    public static class ProxyManager
    {
        private static readonly Dictionary<object, object> _wrappers = new Dictionary<object, object>();

        public static ProxyWrapper<T> GetWrapper<T>(T proxy)
            where T : class, new()
        {
            object wrapper;

            _wrappers.TryGetValue(proxy, out wrapper);

            return (ProxyWrapper<T>)wrapper;
        }

        internal static void AddWrapper(object proxy, object wrapper)
        {
            _wrappers[proxy] = wrapper;
        }
    }
}

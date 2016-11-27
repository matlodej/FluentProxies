using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies
{
    /// <summary>
    /// Provides additional functionality for a proxy as well as access to the source object the proxy was created from.
    /// </summary>
    /// <typeparam name="T">The type of the proxied object.</typeparam>
    public class ProxyWrapper<T>
        where T : class, new()
    {
        /// <summary>
        /// The reference to the object the proxy was created from.
        /// </summary>
        public T SourceReference { get; private set; }

        internal ProxyWrapper(T sourceReference)
        {
            SourceReference = sourceReference;
        }
    }
}

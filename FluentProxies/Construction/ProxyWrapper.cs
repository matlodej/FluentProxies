using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction.Utils;
using FluentProxies.Enums;

namespace FluentProxies.Construction
{
    public class ProxyWrapper<T>
        where T : class, new()
    {
        #region Fields and properties

        public T SourceReference { get; private set; }

        #endregion

        #region Initialization

        public ProxyWrapper(T proxy, T sourceReference)
        {
            SourceReference = sourceReference;
            ProxyManager.AddWrapper(proxy, this);
        }

        #endregion
    }
}

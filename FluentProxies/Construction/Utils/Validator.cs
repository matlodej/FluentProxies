using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Enums;
using FluentProxies.Exceptions;

namespace FluentProxies.Construction.Utils
{
    internal static class Validator
    {
        #region Methods

        internal static void Check<T>(ProxyBuilder<T> proxyBuilder)
            where T : class, new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (!properties.Any())
            {
                throw new ObjectCannotBeProxiedException("No public non-static properties available.");
            }

            if (properties.Any(x => x.GetSetMethod() != null && !x.GetSetMethod().IsVirtual))
            {
                throw new ObjectCannotBeProxiedException("All public non-static properties must be declared as virtual.");
            }

            if (!Instantiator.IsSerializable(proxyBuilder.SourceReference))
            {
                throw new ObjectCannotBeProxiedException("Object is not serializable.");
            }
        }

        internal static bool IsValid<T>(ProxyBuilder<T> proxyBuilder)
            where T : class, new()
        {
            try
            {
                Check(proxyBuilder);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool AreEqual(ProxyConfiguration pc1, ProxyConfiguration pc2)
        {
            return pc1.SourceType == pc2.SourceType
                && pc1.SyncsWithReference == pc2.SyncsWithReference
                && pc1.Implementers.Count == pc2.Implementers.Count
                && pc1.Implementers.All(x => pc2.Implementers.Contains(x));
        }

        #endregion
    }
}

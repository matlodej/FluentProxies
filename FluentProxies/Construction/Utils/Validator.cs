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
        internal static void Check<T>(ProxyBuilder<T> proxyBuilder)
            where T : class, new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (!properties.Any())
            {
                throw new ObjectCannotBeProxiedException("No public non-static property setters available.");
            }

            if (!Instantiator.IsSerializable(proxyBuilder.SourceObject))
            {
                throw new ObjectCannotBeProxiedException("Object is not serializable.");
            }

            if (proxyBuilder.TypesToImplement.Contains(typeof(INotifyPropertyChanged))
                && properties.All(x => x.GetSetMethod() == null || !x.GetSetMethod().IsVirtual))
            {
                throw new InvalidConfigurationException("To create a proxy implementing INotifyPropertyChanged interface there has to be at least one public non-static virtual property setter available.");
            }

            if (proxyBuilder.IncludesWrapper
                && properties.Any(x => x.GetSetMethod() != null && !x.GetSetMethod().IsVirtual))
            {
                throw new InvalidConfigurationException("To create a proxy wrapper all public non-static property setters must be virtual.");
            }

            if (!proxyBuilder.IncludesWrapper && proxyBuilder.SyncsWithSourceObject)
            {
                throw new InvalidConfigurationException("Syncing with source object is only available for wrapper proxies. Call .IncludeWrapper() to create a wrapper proxy.");
            }

            if (proxyBuilder.SyncsWithSourceObject && properties.Any(x => !x.PropertyType.IsValueType && x.PropertyType != typeof(String)))
            {
                throw new InvalidConfigurationException("Syncing with source object is not available for objects with reference type properties other than System.String.");
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
    }
}

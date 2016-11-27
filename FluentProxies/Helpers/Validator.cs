using FluentProxies.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Helpers
{
    internal static class Validator
    {
        internal static void Check<T>(ProxyBuilder<T> proxyBuilder)
            where T : class, new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (!properties.Any())
            {
                throw new ObjectCannotBeProxiedException("No public non-static properties available.");
            }

            if (properties.Any(x => (x.GetGetMethod() != null && !x.GetGetMethod().IsVirtual)
                || (x.GetSetMethod() != null && !x.GetSetMethod().IsVirtual)))
            {
                throw new ObjectCannotBeProxiedException("All public non-static properties must be declared as virtual.");
            }

            if (!IsSerializable(proxyBuilder.SourceReference))
            {
                throw new ObjectCannotBeProxiedException("Object is not serializable.");
            }

            if (proxyBuilder.Blueprint.Properties.Count != proxyBuilder.Blueprint.Properties.Select(x => x.Name).Distinct().Count())
            {
                throw new ObjectCannotBeProxiedException("You cannot declare two properties with the same name.");
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

        internal static bool IsSerializable<T>(T sourceObject)
            where T : class, new()
        {
            return Instantiator.TryClone(sourceObject, out T result);
        }

        internal static bool AreEqual(ProxyBlueprint A, ProxyBlueprint B)
        {
            return A.SourceType == B.SourceType
                && A.SyncsWithReference == B.SyncsWithReference
                && A.Implementations == B.Implementations
                && A.Properties.Count == B.Properties.Count
                && A.Properties.All(x => B.Properties.Any(y => AreEqual(x, y)))
                && A.Interfaces.Count == B.Interfaces.Count
                && A.Interfaces.All(x => B.Interfaces.Any(y => AreEqual(x, y)));
        }

        internal static bool AreEqual(PropertyModel A, PropertyModel B)
        {
            return A.Name == B.Name
                && A.Type == B.Type;
        }

        internal static bool AreEqual(InterfaceModel A, InterfaceModel B)
        {
            return A.Type == B.Type;
        }
    }

    public class ObjectCannotBeProxiedException : Exception
    {
        public ObjectCannotBeProxiedException()
        {
        }

        public ObjectCannotBeProxiedException(string message)
        : base(message)
        {
        }

        public ObjectCannotBeProxiedException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}

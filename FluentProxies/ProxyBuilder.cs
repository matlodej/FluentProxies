using FluentProxies.Enums;
using FluentProxies.Helpers;
using FluentProxies.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies
{
    /// <summary>
    /// Provides methods for creating proxies and retrieving proxy wrappers.
    /// </summary>
    public static class ProxyBuilder
    {
        /// <summary>
        /// Creates a proxy of a provided object.
        /// </summary>
        /// <typeparam name="T">The type of the proxied object.</typeparam>
        /// <param name="sourceObject">The object to create a proxy from.
        /// Must have at least one public non-static property, and all public non-static properties must be virtual.</param>
        public static ProxyBuilder<T> CreateProxy<T>(T sourceObject)
            where T : class, new()
        {
            return new ProxyBuilder<T>(sourceObject);
        }

        /// <summary>
        /// Retrieves the wrapper of a given proxy.
        /// </summary>
        /// <typeparam name="T">The type of the proxied object.</typeparam>
        /// <param name="proxy">The proxy to retrieve the wrapper from.</param>
        public static ProxyWrapper<T> GetWrapper<T>(T proxy)
            where T : class, new()
        {
            Cache.Wrappers.TryGet(proxy, out object wrapper);
            return (ProxyWrapper<T>)wrapper;
        }
    }

    /// <summary>
    /// Provides methods for creating proxies and retrieving proxy wrappers.
    /// </summary>
    public class ProxyBuilder<T>
        where T : class, new()
    {
        internal T SourceReference { get; private set; }

        internal ProxyBlueprint Blueprint { get; private set; } = new ProxyBlueprint();

        /// <summary>
        /// Checks for errors in the current builder configuration.
        /// In some cases the build method may still throw an exception even if configuration appears to be valid
        /// (for example if you add an interface to a proxy without providing all of the necessary interface members).
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Validator.IsValid(this);
            }
        }

        internal ProxyBuilder(T sourceReference)
        {
            SourceReference = sourceReference;
        }

        /// <summary>
        /// Makes it so every change to a property on a proxy modifes the correlating property on the source object the proxy was created from.
        /// Likewise, if the original object is modified outside of the proxy, the proxy will return the modified value.
        /// The proxy reference itself has no values, the property get and set methods are linked to the source object reference.
        /// </summary>
        public ProxyBuilder<T> SyncWithReference()
        {
            Blueprint.SyncsWithReference = true;
            return this;
        }

        /// <summary>
        /// Adds one of the predefined implementations to a proxy.
        /// </summary>
        /// <param name="implementations">The implementation to be added to a proxy.</param>
        public ProxyBuilder<T> Implement(Implementations implementations)
        {
            Blueprint.Implementations |= implementations;
            return this;
        }

        /// <summary>
        /// Adds a custom public property to a proxy.
        /// </summary>
        /// <typeparam name="TPropertyType">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        public ProxyBuilder<T> AddProperty<TPropertyType>(string propertyName)
        {
            PropertyModel propertyModel = new PropertyModel
            {
                Name = propertyName,
                Type = typeof(TPropertyType),
            };

            Blueprint.Properties.Add(propertyModel);
            return this;
        }

        /// <summary>
        /// Adds a custom interface to a proxy. If the proxy does not provide all of the required interface members, the build method will throw an exception.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        public ProxyBuilder<T> AddInterface<TInterface>()
            where TInterface : class
        {
            InterfaceModel interfaceModel = new InterfaceModel
            {
                Type = typeof(TInterface),
            };

            Blueprint.Interfaces.Add(interfaceModel);
            return this;
        }

        /// <summary>
        /// Builds the proxy using the current builder configuration.
        /// </summary>
        public T Build()
        {
            Validator.Check(this);
            return new ProxyConstructor<T>(this).Construct();
        }
    }
}

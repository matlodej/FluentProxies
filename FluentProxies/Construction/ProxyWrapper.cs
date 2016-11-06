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
        private readonly ProxyBuilder<T> _builder;

        private readonly T _proxy;

        private readonly ObjectInspector<T> _objectInspector;

        public T SourceObject { get; private set; }

        public ProxyState State
        {
            get
            {
                if (_builder.SyncsWithSourceObject)
                {
                    return ProxyState.InSync;
                }

                if (_objectInspector.AreEqual())
                {
                    return ProxyState.Unmodified;
                }

                return ProxyState.Modified;
            }
        }

        public List<PropertyInfo> IncludedProperties
        {
            get
            {
                return _objectInspector.GetIncludedProperties();
            }
        }

        public List<PropertyInfo> OmittedProperties
        {
            get
            {
                return _objectInspector.GetOmittedProperties();
            }
        }

        public List<PropertyDifference> Changes
        {
            get
            {
                return _objectInspector.GetDifferences();
            }
        }

        public ProxyWrapper(ProxyBuilder<T> builder, T proxy)
        {
            _builder = builder;
            _proxy = proxy;
            _objectInspector = new ObjectInspector<T>(builder.SourceObject, proxy);

            SourceObject = builder.SourceObject;

            ProxyManager.AddWrapper(proxy, this);
        }

        public void CommitChanges()
        {
            _objectInspector.CommitChangesToSource();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction;
using FluentProxies.Construction.Utils;
using FluentProxies.Enums;

namespace FluentProxies
{
    public static class ProxyBuilder
    {
        public static ProxyBuilder<T> CreateProxy<T>(T sourceObject)
            where T : class, new()
        {
            return new ProxyBuilder<T>(sourceObject);
        }
    }

    public class ProxyBuilder<T>
        where T : class, new()
    {
        internal T SourceObject { get; private set; }

        internal bool IncludesWrapper { get; private set; }

        internal bool SyncsWithSourceObject { get; private set; }

        internal List<Type> TypesToImplement { get; } = new List<Type>();

        internal bool OverridesCache { get; private set; }

        public bool IsValid
        {
            get
            {
                return Validator.IsValid(this);
            }
        }

        internal ProxyBuilder(T sourceObject)
        {
            SourceObject = sourceObject;
        }

        public ProxyBuilder<T> IncludeWrapper()
        {
            IncludesWrapper = true;
            return this;
        }

        public ProxyBuilder<T> SyncWithSourceObject()
        {
            SyncsWithSourceObject = true;
            return this;
        }

        public ProxyBuilder<T> Implement(Implementations implementations)
        {
            if (implementations.HasFlag(Implementations.INotifyPropertyChanged)
                && !TypesToImplement.Contains(typeof(INotifyPropertyChanged)))
            {
                TypesToImplement.Add(typeof(INotifyPropertyChanged));
            }

            return this;
        }

        public ProxyBuilder<T> OverrideCache()
        {
            OverridesCache = true;
            return this;
        }

        public T Build()
        {
            Validator.Check(this);
            return new ProxyConstructor<T>(this).Construct();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction;
using FluentProxies.Construction.Implementers;
using FluentProxies.Construction.Utils;
using FluentProxies.Enums;
using FluentProxies.Models;

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
        #region Fields and properties

        internal T SourceReference { get; private set; }

        internal ProxyConfiguration Configuration { get; private set; }

        public bool IsValid
        {
            get
            {
                return Validator.IsValid(this);
            }
        }

        #endregion

        #region Initialization

        internal ProxyBuilder(T sourceReference)
        {
            SourceReference = sourceReference;
            Configuration = new ProxyConfiguration();
        }

        #endregion

        #region Methods

        public ProxyBuilder<T> SyncWithReference()
        {
            Configuration.SyncsWithReference = true;
            return this;
        }

        public ProxyBuilder<T> Implement(Implementations implementations)
        {
            Configuration.Implementations |= Implementations.INotifyPropertyChanged;
            return this;
        }

        public T Build()
        {
            Validator.Check(this);
            return new ProxyConstructor<T>(this).Construct();
        }

        #endregion
    }
}

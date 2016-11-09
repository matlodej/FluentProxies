using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Construction.Implementers
{
    public abstract class Implementer
    {
        #region Fields and properties

        internal Type Interface { get; private set; }

        #endregion

        #region Initialization

        public Implementer(Type interfaceType)
        {
            Interface = interfaceType;
        }

        #endregion

        #region Methods

        internal virtual void Implement(TypeBuilder typeBuilder)
        {
        }

        internal virtual void BeforeGet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }

        internal virtual void BeforeSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }

        internal virtual void AfterSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }

        #endregion
    }
}

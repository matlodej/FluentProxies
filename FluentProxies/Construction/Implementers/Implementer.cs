using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Enums;

namespace FluentProxies.Construction.Implementers
{
    internal abstract class Implementer
    {
        internal static List<Implementer> Resolve(Implementations implementations)
        {
            List<Implementer> implementers = new List<Implementer>();

            if (implementations.HasFlag(Implementations.INotifyPropertyChanged))
            {
                implementers.Add(new INotifyPropertyChangedImplementer());
            }

            return implementers;
        }

        internal abstract Type Interface { get; }

        internal abstract void Implement(TypeBuilder typeBuilder);

        internal virtual void BeforeGet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }

        internal virtual void BeforeSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }

        internal virtual void AfterSet(ILGenerator gen, PropertyInfo propertyInfo)
        {
        }
    }
}

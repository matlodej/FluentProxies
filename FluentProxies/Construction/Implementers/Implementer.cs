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
    internal abstract class Implementer
    {
        internal static Implementer Resolve(Type typeToImplement)
        {
            if (typeToImplement == typeof(INotifyPropertyChanged))
            {
                return new INotifyPropertyChangedImplementer();
            }

            throw new ArgumentOutOfRangeException(nameof(typeToImplement));
        }

        internal abstract Implementer Implement(TypeBuilder typeBuilder);

        internal virtual Action<ILGenerator, PropertyInfo> BeforeGet { get; set; }
            = (x, y) => { };

        internal virtual Action<ILGenerator, PropertyInfo> BeforeSet { get; set; }
            = (x, y) => { };

        internal virtual Action<ILGenerator, PropertyInfo> AfterSet { get; set; }
            = (x, y) => { };
    }
}

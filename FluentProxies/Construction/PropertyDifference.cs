using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Construction
{
    public class PropertyDifference
    {
        public PropertyInfo Property { get; private set; }

        public object SourceValue { get; private set; }

        public object ProxyValue { get; private set; }

        public PropertyDifference(PropertyInfo property,
            object sourceValue,
            object proxyValue)
        {
            Property = property;
            SourceValue = sourceValue;
            ProxyValue = proxyValue;
        }
    }
}

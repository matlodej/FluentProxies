using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Construction.Utils
{
    internal class ObjectInspector<T>
        where T : class, new()
    {
        private readonly T _sourceObject;

        private readonly T _proxyObject;

        public ObjectInspector(T sourceObject, T proxyObject)
        {
            _sourceObject = sourceObject;
            _proxyObject = proxyObject;
        }

        internal bool AreEqual()
        {
            return !GetDifferences().Any();
        }

        internal List<PropertyDifference> GetDifferences()
        {
            List<PropertyDifference> differences = new List<PropertyDifference>();

            List<PropertyInfo> properties = GetIncludedProperties();

            foreach (PropertyInfo property in properties)
            {
                object sVal = property.GetValue(_sourceObject);
                object pVal = property.GetValue(_proxyObject);

                if (sVal == null && pVal == null)
                {
                    continue;
                }

                if ((sVal == null || pVal == null)
                    || (!sVal.Equals(pVal)))
                {
                    differences.Add(new PropertyDifference(property, sVal, pVal));
                }
            }

            return differences;
        }

        internal List<PropertyInfo> GetIncludedProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.PropertyType.IsValueType || x.PropertyType == typeof(String))
                .ToList();
        }

        internal List<PropertyInfo> GetOmittedProperties()
        {
            return typeof(T).GetProperties()
                .Except(GetIncludedProperties())
                .ToList();
        }

        internal void CommitChangesToSource()
        {
            List<PropertyDifference> properties = GetDifferences();

            foreach (PropertyDifference property in properties)
            {
                property.Property.SetValue(_sourceObject, property.ProxyValue);
            }
        }
    }
}

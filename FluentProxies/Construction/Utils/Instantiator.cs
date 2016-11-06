using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Construction.Utils
{
    internal static class Instantiator
    {
        internal static T Clone<T>(T sourceObject, Type targetType)
            where T : class, new()
        {
            MemoryStream ms = new MemoryStream();
            DateTimeFormat dateFormat = new DateTimeFormat("yyyy-MM-dd HH:mm:ss:fffffff");

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { DateTimeFormat = dateFormat });
            serializer.WriteObject(ms, sourceObject);

            ms.Position = 0;

            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(targetType, new DataContractJsonSerializerSettings { DateTimeFormat = dateFormat });

            T clone = (T)deserializer.ReadObject(ms);
            
            return clone;
        }

        internal static bool TryClone<T>(T sourceObject, Type targetType, out T clone)
            where T : class, new()
        {
            try
            {
                clone = Clone(sourceObject, targetType);
                return true;
            }
            catch
            {
                clone = default(T);
                return false;
            }
        }

        internal static T Clone<T>(T sourceObject)
            where T : class, new()
        {
            return Clone(sourceObject, typeof(T));
        }

        internal static bool TryClone<T>(T sourceObject, out T clone)
            where T : class, new()
        {
            return TryClone(sourceObject, typeof(T), out clone);
        }

        internal static bool IsSerializable<T>(T sourceObject)
            where T : class, new()
        {
            T result;
            return TryClone(sourceObject, out result);
        }
    }
}

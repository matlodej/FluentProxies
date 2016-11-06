using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Exceptions
{
    public class ObjectCannotBeProxiedException : Exception
    {
        public ObjectCannotBeProxiedException()
        {
        }

        public ObjectCannotBeProxiedException(string message)
        : base(message)
        {
        }

        public ObjectCannotBeProxiedException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Exceptions
{
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException()
        {
        }

        public InvalidConfigurationException(string message)
        : base(message)
        {
        }

        public InvalidConfigurationException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}

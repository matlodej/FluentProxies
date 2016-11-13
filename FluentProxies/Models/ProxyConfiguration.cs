using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction.Implementers;
using FluentProxies.Enums;

namespace FluentProxies.Models
{
    internal class ProxyConfiguration
    {
        internal Type SourceType { get; set; }

        internal bool SyncsWithReference { get; set; }

        internal Implementations Implementations { get; set; }
    }
}

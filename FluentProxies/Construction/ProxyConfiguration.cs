using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentProxies.Construction.Implementers;

namespace FluentProxies.Construction
{
    internal class ProxyConfiguration
    {
        internal Type SourceType { get; set; }

        internal bool SyncsWithReference { get; set; }

        internal List<Implementer> Implementers { get; } = new List<Implementer>();
    }
}

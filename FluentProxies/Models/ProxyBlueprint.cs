using FluentProxies.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Models
{
    internal class ProxyBlueprint
    {
        internal Type SourceType { get; set; }

        internal bool SyncsWithReference { get; set; }

        internal Implementations Implementations { get; set; }

        internal List<PropertyModel> Properties { get; private set; } = new List<PropertyModel>();

        internal List<InterfaceModel> Interfaces { get; private set; } = new List<InterfaceModel>();
    }
}

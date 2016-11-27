using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentProxies.Enums
{
    /// <summary>
    /// Predefined implementations that can be added to a proxy.
    /// </summary>
    [Flags]
    public enum Implementations
    {
        /// <summary>
        /// Implements the INotifyPropertyChanged interface on all public non-static properties.
        /// </summary>
        INotifyPropertyChanged = 1,
    }
}

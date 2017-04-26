using System;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Input
{
    /// <summary>
    /// Port with a generic DataType - be carefull since this port is not strictly typed ... 
    /// </summary>
    public class ObjectPort: ExtendedPort
    {
        public ObjectPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            DataType = typeof(object); 

        }
    }
}

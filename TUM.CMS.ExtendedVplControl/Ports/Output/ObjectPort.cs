using System;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Output
{
    /// <summary>
    /// Port with a generic DataType - be carefull since this port is not strictly typed ... 
    /// </summary>
    public class ObjectPort: ExtendedPort
    {
        private WatchPortControl portControl;

        public ObjectPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            DataType = typeof(object);

            portControl = new WatchPortControl();

            AddPopupContent(portControl);

            DataChanged += OnDataChanged;
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            portControl.Data = Data;
        }
    }
}

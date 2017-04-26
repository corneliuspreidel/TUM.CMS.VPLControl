using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Output
{
    /// <summary>
    /// String Data Port
    /// </summary>
    public class StringPort: ExtendedPort
    {
        private WatchPortControl portControl;

        public StringPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(string);
            portControl = new WatchPortControl();

            AddPopupContent(portControl);

            DataChanged += OnDataChanged;
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            portControl.Data = Data;
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Data");
            if(Data != null)
                xmlWriter.WriteValue(Data);
            xmlWriter.WriteEndAttribute();
            base.SerializeNetwork(xmlWriter);
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);
            var value = xmlReader.GetAttribute("Data");
            if (value != null)
                Data = value;
        }
    }
}

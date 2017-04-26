using System;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Utilities;
using TUM.CMS.VplControl.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Output
{
    public class IntegerPort : ExtendedPort
    {
        private WatchPortControl portControl;

        public IntegerPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas)
            : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(int);
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
            else
                xmlWriter.WriteValue("");
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

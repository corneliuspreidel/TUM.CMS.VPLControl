using System;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Input
{
    /// <summary>
    /// String Data Port
    /// </summary>
    public class StringPort: ExtendedPort
    {
        public StringPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(string);

            var textBox = new TextBox {MinWidth = 50};
            AddPopupContent(textBox);

            if (Data != null)
                textBox.Text = (string) Data;

            textBox.TextChanged += TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var textBox = sender as TextBox;
            Data = textBox.Text;
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Data");
            if(Data!=null)
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

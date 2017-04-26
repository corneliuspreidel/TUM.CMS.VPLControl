using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using TUM.CMS.VplControl.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Input
{
    public class IntegerPort: ExtendedPort
    {
        private SliderExpanderInteger integerSlider;

        public IntegerPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(int);

            integerSlider = new SliderExpanderInteger
            {
                Style = hostCanvas.FindResource("ExpanderSliderStyleInteger") as Style,
                SliderValue = 5,
                SliderMax = 10,
                SliderMin = 2,
                SliderStep = 1
            };

            if (Data != null)
                integerSlider.SliderValue = (int)Data;

            var binding = new Binding("Data")
            {
                Mode = BindingMode.OneWayToSource,
                Source = this
            };

            integerSlider.SetBinding(SliderExpanderInteger.SliderValueProperty, binding);
            AddPopupContent(integerSlider);
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Data");
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

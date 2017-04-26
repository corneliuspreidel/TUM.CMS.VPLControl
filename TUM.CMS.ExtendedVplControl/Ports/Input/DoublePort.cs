using System;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using TUM.CMS.VplControl.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Input
{
    public class DoublePort: ExtendedPort
    {
        private SliderExpanderDouble doubleSlider;

        public DoublePort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(double);
            
            doubleSlider = new SliderExpanderDouble
            {
                Style = hostCanvas.FindResource("ExpanderSliderStyleDouble") as Style,
                SliderValue = 5,
                SliderMax = 10,
                SliderMin = 2,
                SliderStep = 0.01
            };

            var binding = new Binding("Data")
            {
                Mode = BindingMode.OneWayToSource,
                Source = this
            };

            doubleSlider.SetBinding(SliderExpanderDouble.SliderValueProperty, binding);

            AddPopupContent(doubleSlider);
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Data");
            if (Data == null)
                return; 
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

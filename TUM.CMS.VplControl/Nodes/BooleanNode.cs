using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;

namespace TUM.CMS.VplControl.Nodes
{
    public class BooleanNode : Node
    {
        public ToggleButton toggleButton { get; set; }

        public BooleanNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            AddOutputPortToNode("Boolean", typeof (bool));

            toggleButton = new ToggleButton
            {
                Width = 100,
                Content = true,
                IsChecked = true
            };

            toggleButton.Unchecked += ToggleButtonChechedStateChanged;
            toggleButton.Checked += ToggleButtonChechedStateChanged;

            AddControlToNode(toggleButton);
        }

        private void ToggleButtonChechedStateChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            if (toggleButton.IsChecked == false)
            {
                toggleButton.Content = false;
            }
            if (toggleButton.IsChecked == true)
            {
                toggleButton.Content = true;
            }
            Calculate();
        }

        public override void Calculate()
        {
                OutputPorts[0].Data = toggleButton.IsChecked;
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            var textBox = ControlElements[0] as ToggleButton;
            if (textBox == null) return;

            xmlWriter.WriteStartAttribute("Boolean");
            xmlWriter.WriteValue(toggleButton.IsChecked.ToString());
            xmlWriter.WriteEndAttribute();
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            var textBox = ControlElements[0] as ToggleButton;
            if (textBox == null) return;

            toggleButton.IsChecked = Convert.ToBoolean(xmlReader.GetAttribute("Boolean"));
        }

        public override Node Clone()
        {
            return new TextNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
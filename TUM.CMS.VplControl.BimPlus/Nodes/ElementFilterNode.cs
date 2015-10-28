
using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;
using BimPlus.IntegrationFramework.Contract.Model;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementFilterNode : Node
    {
        private readonly DataController _controller;
        private readonly ListBox _filterComboBox;

        public ElementFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Add ComboBox to the control ... 
            _filterComboBox = new ListBox()
            {
                MaxHeight = 200,
                MinWidth = 100,
                SelectionMode = SelectionMode.Multiple
            };

            _filterComboBox.SelectionChanged += FilterComboBoxOnSelectionChanged;
            AddControlToNode(_filterComboBox);

            AddInputPortToNode("Input", typeof (object));
            AddOutputPortToNode("FilteredElements", typeof(object));

            DataContext = this;
        }

        public override void Calculate()
        {
            // Check values ... 
            if (InputPorts[0].Data == null) return;
            if (InputPorts[0].Data.GetType() != typeof(List<GenericElement>)) return;

            // Init the ComboBox 
            _filterComboBox.Items.Clear();

            // Loop through all the elements
            foreach (var elem in InputPorts[0].Data as List<GenericElement>)
            {
                if (_filterComboBox.Items.Contains(elem.TypeName) == false)
                    _filterComboBox.Items.Add(elem.TypeName);
            }
        }

        private void FilterComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // Check Input
            if (InputPorts[0].Data.GetType() != typeof(List<GenericElement>)) return;
            if (_filterComboBox.SelectedItem == null) return;

            // Filter for types
            var filteredElements = new List<GenericElement>();
            foreach (var type in _filterComboBox.SelectedItems)
            {
                foreach (var elem in InputPorts[0].Data as List<GenericElement>)
                {
                    if (elem != null && elem.TypeName == type.ToString())
                    {
                        filteredElements.Add(elem);
                    }
                }
            }

            OutputPorts[0].Data = filteredElements;
        }

        public override Node Clone()
        {
            return new ElementFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            var _filterComboBox = ControlElements[0] as ListBox;
            if (_filterComboBox == null) return;

            xmlWriter.WriteStartAttribute("SelectedItems");
            xmlWriter.WriteValue(_filterComboBox.SelectedItems);
            xmlWriter.WriteEndAttribute();
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            var _filterComboBox = ControlElements[0] as ListBox;
            if (_filterComboBox == null) return;

            var selectedItems = xmlReader.GetAttribute("SelectedItems");

            // if()
            // _filterComboBox.SelectedItems = xmlReader.GetAttribute("SelectedItems");
        }
    }
}
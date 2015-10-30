using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using BimPlus.IntegrationFramework.Contract.Model;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementFilterNode : Node
    {
        private readonly DataController _controller;
        private readonly ListBox _filterListBox;

        public ElementFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Add ComboBox to the control ... 
            _filterListBox = new ListBox()
            {
                MaxHeight = 200,
                MinWidth = 100,
                SelectionMode = SelectionMode.Multiple
            };

            _filterListBox.SelectionChanged += FilterListBoxOnSelectionChanged;
            AddControlToNode(_filterListBox);

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
            _filterListBox.Items.Clear();

            // Loop through all the elements
            var genericElements = InputPorts[0].Data as List<GenericElement>;
            if (genericElements == null) return;
            foreach (var elem in genericElements.Where(elem => _filterListBox.Items.Contains(elem.TypeName) == false))
            {
                _filterListBox.Items.Add(elem.TypeName);
            }
        }

        private void FilterListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // Check Input
            if (InputPorts[0].Data.GetType() != typeof(List<GenericElement>)) return;
            if (_filterListBox.SelectedItem == null) return;

            // Filter for types
            var filteredElements = new List<GenericElement>();
            foreach (var type in _filterListBox.SelectedItems)
            {
                var genericElements = InputPorts[0].Data as List<GenericElement>;
                if (genericElements == null) continue;
                filteredElements.AddRange(genericElements.Where(elem => elem != null && elem.TypeName == type.ToString()));
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

            var filterComboBox = ControlElements[0] as ListBox;
            if (filterComboBox == null) return;

            xmlWriter.WriteStartAttribute("SelectedItems");
            xmlWriter.WriteValue(filterComboBox.SelectedItems);
            xmlWriter.WriteEndAttribute();
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);

            var listBox = ControlElements[0] as ListBox;
            if (listBox == null) return;

            var selectedItems = xmlReader.GetAttribute("SelectedItems");

            if (selectedItems != null)
                foreach (var item in selectedItems)
                {
                    if (_filterListBox.Items.Contains(item))
                        _filterListBox.SelectedItems.Add(item);
                }
            
        }
    }
}
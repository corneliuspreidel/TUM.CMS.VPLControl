using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using BimPlus.Sdk.Data.DbCore;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementFilterNode : OperatorObjectNode
    {
        private readonly DataController _controller;
        private readonly ListBox _filterListBox;

        private ModelInfo _modelInfo;
        private List<DtObject> _elements;

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

            AddInputPortToNode("Input", typeof (ModelInfo));
            AddOutputPortToNode("FilteredElements", typeof(ModelInfo));

            DataContext = this;
        }

        public override void Calculate()
        {
            // Check values ... 
            if (InputPorts[0].Data == null) return;

            // ModelInfo
            if (InputPorts[0].Data.GetType() != typeof(ModelInfo)) return;
            // Loop through all the elements
            _modelInfo = InputPorts[0].Data as ModelInfo;
            if (_modelInfo == null) return;

            _elements = new List<DtObject>();

            if (_modelInfo.ModelType == ModelType.BimPlusModel)
            {
                // Get the corresponding model
                var model = _controller.BimPlusModels[Guid.Parse(_modelInfo.ModelId)];
                if (model == null) return;
                _elements = model.Objects as List<DtObject>;
            }
            
            if (_elements == null) return;
            foreach (var elem in _elements.Where(elem => _filterListBox.Items.Contains(elem.Type) == false))
            {
                _filterListBox.Items.Add(elem.Type);
            }
        }

        private void FilterListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // Filter for types
            var filteredElements = new List<DtObject>();
            foreach (var type in _filterListBox.SelectedItems)
            {
                if (_elements == null) continue;
                filteredElements.AddRange(_elements.Where(elem => elem != null && elem.Type == type.ToString()));
            }

            // Set the ModelInfo Output
            var output = new ModelInfo(_modelInfo.ProjectId, _modelInfo.ModelId, filteredElements.Select(item => item.Id).ToList(), ModelType.BimPlusModel);
            OutputPorts[0].Data = output;
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

            if (selectedItems == null) return;

            foreach (var item in selectedItems)
            {
                if (_filterListBox.Items.Contains(item))
                    _filterListBox.SelectedItems.Add(item);
            }
        }
    }
}
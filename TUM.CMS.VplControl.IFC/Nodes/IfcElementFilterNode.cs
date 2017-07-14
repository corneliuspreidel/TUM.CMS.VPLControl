using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcElementFilterNode : OperatorObjectNode
    {
        private readonly ModelController _controller;

        private ModelInfo _modelInfo;
        private List<IIfcProduct> _elements;

        private Xbim3DModelContext context;
        private IfcStore xModel;

        private IfcElementFilterControl _control;

        private List<Type> _typeList;

        public IfcElementFilterNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            _typeList = new List<Type>();

            _control = new IfcElementFilterControl();
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = ModelController.Instance;

            _control._filterListBox.SelectionChanged += FilterListBoxOnSelectionChanged;
            AddControlToNode(_control);

            _control._searchTextBox.OnSearch += SearchTextBoxOnOnSearch;

            AddInputPortToNode("Input", typeof (ModelInfo));
            AddOutputPortToNode("FilteredElements", typeof(ModelInfo));

            DataContext = this;
        }

        private void SearchTextBoxOnOnSearch(object sender, RoutedEventArgs e)
        {
            var searchArgs = e as SearchEventArgs;

            if (searchArgs == null) return;

            if (_typeList.Count == 0)
                return; 

            if (searchArgs.Keyword == "")
            {
                _control._filterListBox.ItemsSource = _typeList;
                _control._filterListBox.SelectedIndex = -1;
            }
            else
            {
                _control._filterListBox.ItemsSource = null;
                _control._filterListBox.Items.Clear();
                _control._filterListBox.ItemsSource = _typeList
                    .Where(x => x.Name.Contains(searchArgs.Keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                _control._filterListBox.SelectedIndex = 0;
            }
        }

        public override void Calculate()
        {
            // Check values ... 
            if (InputPorts[0].Data == null) return;

            if (InputPorts[0].Data.GetType() != typeof(ModelInfo))
                return;
            // ModelInfo
            // if (!InputPorts[0].Data.GetType().IsSubclassOf(typeof(ModelInfo))) return;
            // Loop through all the elements
            _modelInfo = InputPorts[0].Data as ModelInfo;
            if (_modelInfo == null) return;


            var model = _controller.GetModel(_modelInfo.modelId) as IfcModel;
            if (model == null)
                return;
            xModel = model.GetModel();
            // context = model.xModelContext;
            

            var elements = xModel.Instances.OfType<IIfcProduct>();
            _elements = new List<IIfcProduct>();
            foreach (var persistEntity in elements)
            {
                var item = persistEntity;
                _elements.Add(item);
            }

            if (!_elements.Any())
                return;

            if (_elements == null) return;
            foreach (var elem in _elements.Where(elem => _control._filterListBox.Items.Contains(elem.GetType()) == false))
            {
                _control._filterListBox.Items.Add(elem.GetType());
                _typeList.Add(elem.GetType());
            }
        }

        private void FilterListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // Filter for types
            var filteredElements = new List<IIfcProduct>();
            foreach (var type in _control._filterListBox.SelectedItems)
            {
                if (_elements == null) continue;
                filteredElements.AddRange(_elements.Where(elem => elem != null && elem.GetType().ToString() == type.ToString()));
            }

            // Set the ModelInfo Output
            var output = new ModelInfo(_modelInfo.modelId, filteredElements.Select(item => item.GlobalId.ToString()).ToList(), ModelTypes.IFC);
            OutputPorts[0].Data = output;
        }

        public override Node Clone()
        {
            return new IfcElementFilterNode(HostCanvas)
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
                if (_control._filterListBox.Items.Contains(item))
                    _control._filterListBox.SelectedItems.Add(item);
            }
        }
    }
}
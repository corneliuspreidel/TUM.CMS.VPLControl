using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using BimPlus.Sdk.Data.DbCore;
using BimPlus.Sdk.Data.TenantDto;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class PropertyNode: OperatorObjectNode, INotifyPropertyChanged
    {
        // DataController
        private readonly DataController _controller;
        private readonly PropertyNodeControl _control;

        private List<DtObject> _filteredElements;
        private List<string> _attributeGroups;
        private List<string> _attributes;

        private List<DtObject> objectList; 

        public PropertyNode(Core.VplControl hostCanvas): base(hostCanvas)
        { 
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Ports 
            AddInputPortToNode("Element", typeof(object));
            AddOutputPortToNode("Property", typeof(object));

            // Init the Control 
            _control = new PropertyNodeControl
            {
                DataContext = this
            };

            AddControlToNode(_control);
            // EventHandlers 
            _control.ElementTypeListBox.SelectionChanged += ElementTypeListBoxOnSelectionChanged;
            _control.AttributeGroupListBox.SelectionChanged += AttributeGroupListBoxOnSelectionChanged;
            _control.AttributesListBox.SelectionChanged += AttributesListBoxOnSelectionChanged;

            _control.FilterTextBox.SelectionChanged += FilterTextBoxOnSelectionChanged;
        }



        private void ElementTypeListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            _filteredElements = new List<DtObject>();

            if (_control.ElementTypeListBox.SelectedItem != null)
            {
                var selectedElementTypes = (_control.ElementTypeListBox.SelectedItems);
                foreach (var elem in objectList)
                {
                    if (elem != null && selectedElementTypes.Contains(elem.Type))
                    {
                        _filteredElements.Add(elem);
                    }
                }

                OutputPorts[0].Data = _filteredElements;

                // Set as the Filtered Elements
                FilteredElements = _filteredElements;
            }

            // Fill the AttributeGroupList ... 
            if  (_filteredElements.Count != 0)
            {
                _control.AttributeGroupListBox.ItemsSource = _filteredElements[0].AttributeGroups.Keys;
            }
        }

        private void AttributeGroupListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (_control.AttributeGroupListBox.SelectedItem != null)
            {
                _control.AttributesListBox.ItemsSource =
                    _filteredElements[0].AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Keys;
            }
        }

        private void AttributesListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (_control.AttributesListBox.SelectedItem != null)
            {
                var type = _filteredElements[0].AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()][_control.AttributesListBox.SelectedItem.ToString()].GetType();
                _control.CurrentDataTypeLabel.Content = type.ToString();

                // Get the suggested Data of all the model ...
                var data = new List<object>();
                foreach (var item in _filteredElements)
                {
                    if(data.Contains(item.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()][_control.AttributesListBox.SelectedItem.ToString()]) != true)
                        data.Add(item.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()][_control.AttributesListBox.SelectedItem.ToString()]);
                }

                // Set the autocomplete data
                _control.FilterTextBox.ItemsSource = data;

                // Set the first Index automaticallyf if the combox is not empty
                if (_control.FilterTextBox.Items.Count != 0)
                {
                    _control.FilterTextBox.SelectedIndex = 0;
                }
            }
        }

        private void FilterTextBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // First of all check the value of the attribute ... 
            // FilterTextBox
            try
            {
                var resultingElements = new List<DtObject>();

                foreach (var elem in _filteredElements)
                {
                    var attribute = elem.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()][_control.AttributesListBox.SelectedItem.ToString()];
                    if(attribute == _control.FilterTextBox.SelectedItem)
                        resultingElements.Add(elem);
                    /*
                    if (attribute.GetType() == typeof(string))
                    {
                        if ((attribute as string).Contains(_control.FilterTextBox.Text))
                        {
                            resultingElements.Add(elem);
                        }
                    }
                    */
                }

                OutputPorts[0].Data = resultingElements;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// CalculatE Routine ... 
        /// </summary>
        public override void Calculate()
        { 
            if (InputPorts[0].Data == null) return;

            // Check values ... 
            if (InputPorts[0].Data == null) return;
            if (InputPorts[0].Data.GetType() != typeof(ModelInfo)) return;

            var modelInfo = (ModelInfo) InputPorts[0].Data;
            objectList = new List<DtObject>();

            if (modelInfo != null && modelInfo.ModelType == ModelType.BimPlusModel)
            {
                // Get the corresponding model
                var model = _controller.BimPlusModels[Guid.Parse(modelInfo.ModelId)];
                if (model == null) return;

                // _controller.IntBase.APICore.DtObjects.GetDtObjectsWithPropertiesById()

                objectList = model.Objects.Where(item => modelInfo.ElementIds.Contains(item.Id)).ToList();

                var propertyObjectList = _controller.IntBase.ApiCore.DtObjects.GetDtObjectsWithPropertiesById(modelInfo.ElementIds);


                // foreach (var item in objectList)
                // {
                //     var res = _controller.IntBase.APICore.DtObjects.GetDtObjectsWithPropertiesById(item.Id);
                //     resultList.Add(res);
                // }
                // 
                // objectList = resultList;

            }
            // modelInfo.GetCurrentElements()
            // modelInfo.ElementIds

            // Init the ComboBox 
            _control.ElementTypeListBox.Items.Clear();

            // Loop through all the elements
            foreach (var elem in objectList)
            {
                if (_control.ElementTypeListBox.Items.Contains(elem.Type) == false)
                    _control.ElementTypeListBox.Items.Add(elem.Type);
            }
        }

        public override Node Clone()
        {
            return new ProjectNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        #region PropertyChangedHandlers

        public List<DtObject> FilteredElements
        {
            get { return _filteredElements; }
            set
            {
                _filteredElements = value;
                OnPropertyChanged("FilteredElements");
            }
        }

        public List<string> AttributeGroups
        {
            get { return _attributeGroups; }
            set
            {
                _attributeGroups = value;
                OnPropertyChanged("AttributeGroups");
            }
        }
        public List<string> Attributes
        { 
            get { return _attributes; }
            set
            {
                _attributes = value;
                OnPropertyChanged("Attributes");
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        private new void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion PropertyChangedHandlers
    }
}
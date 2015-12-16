using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using BimPlus.IntegrationFramework.Contract.Model;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class PropertyNode: Node, INotifyPropertyChanged
    {
        // DataController
        private readonly DataController _controller;
        private readonly PropertyNodeControl _control;

        private List<GenericElement> _filteredElements;

        private List<string> _attributeGroups;
        private List<string> _attributes;

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
            _filteredElements = new List<GenericElement>();

            if (_control.ElementTypeListBox.SelectedItem != null)
            {
                var selectedElementTypes = (_control.ElementTypeListBox.SelectedItems);
                // var selectedElementTypes = (_control.ElementTypeListBox.SelectedItems as List<string>).Select(elementType => elementType.Name).ToList();

                foreach (var elem in InputPorts[0].Data as List<GenericElement>)
                {
                    if (elem != null && selectedElementTypes.Contains(elem.TypeName))
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
                    _filteredElements[0].AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Attributes.Keys;
            }
        }

        private void AttributesListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (_control.AttributesListBox.SelectedItem != null)
            {
                var type = _filteredElements[0].AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Attributes[_control.AttributesListBox.SelectedItem.ToString()].GetType();
                _control.CurrentDataTypeLabel.Content = type.ToString();

                // Get the suggested Data of all the model ...
                var data = new List<object>();
                foreach (var item in _filteredElements)
                {
                    if(data.Contains(item.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Attributes[_control.AttributesListBox.SelectedItem.ToString()]) != true)
                        data.Add(item.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Attributes[_control.AttributesListBox.SelectedItem.ToString()]);
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
                var resultingElements = new List<GenericElement>();

                foreach (var elem in _filteredElements)
                {
                    var attribute = elem.AttributeGroups[_control.AttributeGroupListBox.SelectedItem.ToString()].Attributes[_control.AttributesListBox.SelectedItem.ToString()];
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
            if (InputPorts[0].Data.GetType() != typeof(List<GenericElement>)) return;

            // Init the ComboBox 
            _control.ElementTypeListBox.Items.Clear();

            // Loop through all the elements
            foreach (var elem in InputPorts[0].Data as List<GenericElement>)
            {
                if (_control.ElementTypeListBox.Items.Contains(elem.TypeName) == false)
                    _control.ElementTypeListBox.Items.Add(elem.TypeName);
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

        public List<GenericElement> FilteredElements
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
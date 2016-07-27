using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Sdk.Data.TenantDto;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ModelNode : DataObjectNode
    {
        // DataController
        private readonly DataController _controller;

        private readonly ComboBox _modelComboBox;

        private DtoDivision _selectedModel;
        private List<DtoDivision> _models;

        public ModelNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            AddInputPortToNode("Project", typeof(object));
            AddOutputPortToNode("Model", typeof(object));

            _modelComboBox = new ComboBox
            {
                DisplayMemberPath = "Name",
                Width = 100,
                Margin = new Thickness(5,20,5,15)
            };
            _modelComboBox.SelectionChanged += SelectionChanged;

            AddControlToNode(_modelComboBox);

            BinButton.Visibility = Visibility.Visible;
        }



        public override void Calculate()
        {
            // Input Part
            if (InputPorts[0].Data.GetType() != typeof (DtoShortProject)) return;
            // _modelComboBox.ItemsSource = null;

            var project = InputPorts[0].Data as DtoShortProject;
            if (project == null) return;
            if (_modelComboBox == null) return;
            _modelComboBox.ItemsSource = _controller.IntBase.APICore.Divisions.GetProjectDivisions(project.Id);
            _modelComboBox.DisplayMemberPath = "Name";

            // Output Part
            // if (_modelComboBox != null) OutputPorts[0].Data = _modelComboBox.SelectedItem as Division;
        }

        public override Node Clone()
        {
            return new ModelNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            if (_modelComboBox != null) OutputPorts[0].Data = _modelComboBox.SelectedItem as DtoDivision;
        }

        #region PropertyChangedHandlers
        public DtoDivision SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                _selectedModel = value;
                OnPropertyChanged("SelectedModel");
            }
        }

        public List<DtoDivision> Models
        {
            get { return _models; }
            set
            {
                _models = value;
                OnPropertyChanged("Models");
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
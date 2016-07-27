using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Client;
using BimPlus.Sdk.Data.DbCore.Structure;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ProjectNode : DataObjectNode
    {
        // DataController
        private readonly DataController _controller;
        private readonly ComboBox _projectComboBox;
        private Project _selectedProject;

        public ProjectNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            _controller.IntBase.EventHandlerCore.TeamChanged += EventHandlerCoreOnTeamChanged;

            if (_controller.IntBase != null)
            {
                _projectComboBox = new ComboBox
                {
                    ItemsSource = _controller.IntBase.APICore.Projects.GetShortProjects(),
                    Width = 100,
                    SelectedItem = _selectedProject,
                    DisplayMemberPath = "Name",
                    Margin = new Thickness(5, 20, 5, 15)
                };

                // Add EventHandler
                _projectComboBox.SelectionChanged += SelectionChanged;

                if (_controller.IntBase != null)
                    AddControlToNode(_projectComboBox);
                else
                    AddControlToNode(new TextBox {Text = "No Connection"});
            }

            AddOutputPortToNode("Project", typeof (object));
        }

        private void EventHandlerCoreOnTeamChanged(object sender, BimPlusEventArgs bimPlusEventArgs)
        {
            _projectComboBox.ItemsSource = _controller.IntBase.APICore.Projects.GetShortProjects();
        }

        public override void Calculate()
        {
            // var project = _projectComboBox.Items.CurrentItem as Project;
            if (_projectComboBox.SelectedItem != null)
                OutputPorts[0].Data = _projectComboBox.SelectedItem;
        }

        public override Node Clone()
        {
            return new ProjectNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            /*
            var project = _projectComboBox.SelectedItem as Project;
            if (project == null) return;
                _controller.DataContainer.SetCurrentProject(project);
            */

            Calculate();
        }

        #region PropertyChangedHandlers

        public Project SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                _selectedProject = value;
                OnPropertyChanged("SelectedProject");
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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Sdk.Data.DbCore.Structure;
using BimPlus.Sdk.Data.TenantDto;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class AttachmentNode : Node
    {
        // DataController
        private readonly DataController _controller;

        private readonly ComboBox _modelComboBox;

        private DtoDivision _selectedModel;
        private List<DtoDivision> _models;

        public AttachmentNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            AddInputPortToNode("Project", typeof(object));
            AddOutputPortToNode("Attachment", typeof(object));

            _modelComboBox = new ComboBox
            {
                DisplayMemberPath = "FileName",
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
            if (InputPorts[0].Data.GetType() != typeof (Project)) return;
            // _modelComboBox.ItemsSource = null;

            var project = InputPorts[0].Data as Project;
            if (project == null) return;
            if (_modelComboBox == null) return;
            // Get the Project Attachments
            // _modelComboBox.ItemsSource = _controller.IntBase.APICore.Attachments.GetObjectAttachments(project.Id);
            _modelComboBox.DisplayMemberPath = "FileName";
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
    }
}
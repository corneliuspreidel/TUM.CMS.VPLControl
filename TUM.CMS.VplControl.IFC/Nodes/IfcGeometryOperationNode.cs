using System;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Practices.Unity;
using QL4BIMspatial;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Utilities;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcGeometryOperationNode : Node
    {
        // DataController
        private readonly ModelController _modelController;
        private readonly ComboBox _typeComboBox;
        private readonly Button _button;

        // QL4BIM Inits
        private static UnityContainer container = new UnityContainer();
        private static MainInterface ql4Spatial = new MainInterface(container);

        public IfcGeometryOperationNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _modelController = ModelController.Instance;

            // Init the QL4BIM framework
            // Commented by CP on 24.04
            var settings = ql4Spatial.GetSettings();
            
            // Input
            AddInputPortToNode("InputElements_1", typeof(ModelInfo));
            // Input
            AddInputPortToNode("InputElements_2", typeof(ModelInfo));
            // Output
            AddOutputPortToNode("Relation", typeof(Relation));

            // Control
            _typeComboBox = new ComboBox();
            _typeComboBox.Items.Add("Overlap");
            // _typeComboBox.Items.Add("Touch");
            // _typeComboBox.Items.Add("Contain");
            // _typeComboBox.Items.Add("AboveOf");
            // _typeComboBox.Items.Add("BelowOf");
            // _typeComboBox.Items.Add("WestOf");
            // _typeComboBox.Items.Add("EastOf");
            // _typeComboBox.Items.Add("NorthOf");
            // _typeComboBox.Items.Add("SouthOf");
            _typeComboBox.SelectedItem = _typeComboBox.Items.GetItemAt(0);
            AddControlToNode(_typeComboBox);

            _button = new Button {Content = "Execute"};
            _button.Click += ButtonOnClick;
            AddControlToNode(_button);
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (InputPorts[0].Data == null || InputPorts[1].Data == null)
                return;

            var modelInfo1 = InputPorts[0].Data as ModelInfo;
            var modelInfo2 = InputPorts[1].Data as ModelInfo;
            if (modelInfo1 != null && modelInfo2 != null)
            {}
            else{return;} 
            
            if (modelInfo1 != null && modelInfo2 != null)
            {
                var res = GeometryOperations.OverlapOperator(modelInfo1, modelInfo2);
                OutputPorts[0].Data = res;
            }
        }

        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return new IfcGeometryOperationNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
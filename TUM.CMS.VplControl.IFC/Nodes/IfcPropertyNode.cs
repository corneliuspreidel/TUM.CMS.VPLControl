using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcPropertyNode: Node
    {
        private ModelController modelController;
        private Xbim3DModelContext context;
        private IfcStore xModel;

        private IfcPropertyNodeControl _control;
        private List<IIfcProduct> _elements;

        public IfcPropertyNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            modelController = ModelController.Instance;
            AddInputPortToNode("ModelInfo", typeof(ModelInfo));

            _control = new IfcPropertyNodeControl {DataContext = this};
            AddControlToNode(_control);

            _control.elementsComboBox.SelectionChanged += ElementsComboBoxOnSelectionChanged;
            IsResizeable = true;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var modelInfo = InputPorts[0].Data as ModelInfo;
            if (modelInfo == null)
                return;

            var model = modelController.GetModel(modelInfo.modelId) as IfcModel;
            if (model == null)
                return;

            // Get the model content
            xModel = model.GetModel();
            context = model.xModelContext;

            _elements =  model.GetElements(modelInfo.elementIds);
            //  _elements = model.GetAllElements();
            _control.elementsComboBox.ItemsSource = _elements;
        }

        private void ElementsComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
                _control.ifcMetaDataControl.SelectedEntity = comboBox.SelectedItem as IIfcProduct;
        }


        public override Node Clone()
        {
            return new IfcPropertyNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}

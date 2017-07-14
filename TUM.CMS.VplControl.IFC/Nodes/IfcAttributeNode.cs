using System;
using System.Collections.Generic;
using System.Linq;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcAttributeNode : Node
    {
        private ModelController modelController;
        public IfcStore xModel;
        private Xbim3DModelContext context;

        private XbimTreeview treeView;

        public IfcAttributeNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;
            AddInputPortToNode("ModelInfo", typeof(object));
            modelController = ModelController.Instance;

            Height = 350;
            Width = 150;
            treeView = new XbimTreeview {MinWidth = 100};
            AddControlToNode(treeView);
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

            treeView = new XbimTreeview();
            treeView.Model = xModel;

            treeView.Regenerate();

            // var properties = AttributeHandler.GetProperties();

        }

        public override Node Clone()
        {
            return new IfcAttributeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}
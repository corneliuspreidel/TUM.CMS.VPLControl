using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc2x3.Interfaces;
using Xbim.Ifc4.UtilityResource;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcViewerNode : Node
    {
        // UI stuff
        private IFCViewerControl ifcViewerControl;

        private IfcModel model;
        private ModelController _controller;

        public IfcViewerNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Init UI
            IsResizeable = true;

            AddInputPortToNode("Model", typeof(ModelInfo));
            AddOutputPortToNode("SelectedElements", typeof(ModelInfo));
        
            ifcViewerControl = new IFCViewerControl();
            AddControlToNode(ifcViewerControl);

            _controller = ModelController.Instance;

            ifcViewerControl.onElementMouseDown += OnElementMouseDown;
        }

        private void OnElementMouseDown(object sender, EventArgs eventArgs)
        {
            var models = sender as Dictionary<string, GeometryModel3D>;

            if (models == null)
                return;

            if (model == null)
                return;

            var modelInfo = new ModelInfo {modelId = model.id.ToString()};

            foreach(var item in models)
            { modelInfo.elementIds.Add(item.Key);}

            OutputPorts[0].Data = modelInfo;

        }

        public override void Calculate()
        {
            ifcViewerControl.InitViewPort();

            if (InputPorts[0].Data == null)
                return;

            if (InputPorts[0].Data.GetType() == typeof(ModelInfo))
            {
                var modelInfo = InputPorts[0].Data as ModelInfo;
                if (modelInfo == null)
                    return;

                model = _controller.GetModel(modelInfo.modelId) as IfcModel;
                if (model == null)
                    return;

                var idList = new List<IfcGloballyUniqueId>();
                foreach (var id in modelInfo.elementIds)
                {
                    var guid = new Guid();
                    if (Guid.TryParse(id, out guid))
                    {
                        idList.Add(IfcGuid.ToIfcGuid(Guid.Parse(id)));
                    }
                    else
                    {
                        idList.Add(new IfcGloballyUniqueId(id));
                    }

                }

                ifcViewerControl.Visualize(ifcViewerControl.CreateModelUiElementsDs(model, idList));
            }
            else if (InputPorts[0].Data.GetType() == typeof(ModelInfo))
            {
                var modelInfo = InputPorts[0].Data as ModelInfo;
                if (modelInfo == null)
                    return;

                model = ModelController.Instance.GetModel(modelInfo.modelId) as IfcModel;

                if (model == null)
                    return;

                var idList = new List<IfcGloballyUniqueId>();

                foreach (var id in modelInfo.elementIds)
                {
                    idList.Add(new IfcGloballyUniqueId(id));
                }

                ifcViewerControl.Visualize(ifcViewerControl.CreateModelUiElementsDs(model, idList));
            }

            else if (InputPorts[0].Data.GetType() == typeof(Relation))
            {
                var relation = InputPorts[0].Data as Relation;

                if (relation == null)
                    return;
                var elements1 = new List<IfcGloballyUniqueId>();
                var elements2 = new List<IfcGloballyUniqueId>();
                foreach (var item in relation.Collection)
                {
                    var item_Tuple = item as Tuple<Guid, Guid>;
                    elements1.Add(IfcGuid.ToIfcGuid(item_Tuple.Item1));
                    elements2.Add(IfcGuid.ToIfcGuid(item_Tuple.Item2));
                }

                var material1 = new DiffuseMaterial { Brush = new SolidColorBrush(Colors.Blue) { Opacity = 0.3 } };

                var material2 = new DiffuseMaterial { Brush = new SolidColorBrush(Colors.Red) { Opacity = 0.3 } };

                // Get the model 
                model = ModelController.Instance.GetModel(relation.ModelId.ToString()) as IfcModel;
                ifcViewerControl.Visualize(ifcViewerControl.CreateModelUiElementsDs(model, elements1, false, material1));
                ifcViewerControl.Visualize(ifcViewerControl.CreateModelUiElementsDs(model, elements2, false, material2), false);
            }

            ifcViewerControl.Viewport3D.ZoomExtents();
        }
       
        public override Node Clone()
        {
            return new IfcViewerNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
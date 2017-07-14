using System.Collections.Generic;
using System.Linq;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcAccessibilityGraph : Node
    {
        public IfcStore xModel;
        private Xbim3DModelContext context;
        private ModelController modelController;
        private IFCViewerControl control;

        public IfcAccessibilityGraph(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            AddInputPortToNode("ModelInfo", typeof(ModelInfo));

            modelController = ModelController.Instance;

            control = new IFCViewerControl();
            AddControlToNode(control);
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

            var storeys = xModel.Instances.OfType<IIfcBuildingStorey>();

            var elements = storeys.FirstOrDefault().ContainsElements.FirstOrDefault().RelatedElements;

            var spaces = storeys.FirstOrDefault().Spaces.ToList();

            // Visualize the identified elements
            control.Visualize(control.CreateModelUiElementsDs(model, spaces.Select(item => item.GlobalId).Distinct().ToList()));

            // var spaces = elements.Where(item => item is IIfcSpace) ;
            // var walls = elements.Where(item => item is IIfcWallStandardCase);
            var stairs  = elements.Where(item => item is IIfcStair).ToList();
            var ramps = elements.Where(item => item is IIfcRamp).ToList();
            var doors = elements.Where(item => item is IIfcDoor).ToList();

            var doorOpenings = new List<IIfcProduct>();
            foreach (var door in doors)
            {
                var ifcRelFillsElement = (door as IIfcDoor).FillsVoids.FirstOrDefault();
                if (ifcRelFillsElement != null)
                    doorOpenings.Add(ifcRelFillsElement.RelatingOpeningElement);
            }

            foreach (var opening in doorOpenings)
            {
                var ifcOpeningElement = opening as IIfcOpeningElement;
                if (ifcOpeningElement != null)
                {
                    var wall = ifcOpeningElement.VoidsElements.RelatingBuildingElement;

                    var info1 = new ModelInfo(modelInfo.modelId, new List<string>() {wall.GlobalId}, ModelTypes.IFC);
                    var info2 = new ModelInfo(modelInfo.modelId, spaces.Select(item => item.GlobalId.ToString()).ToList(), ModelTypes.IFC);

                    var res = GeometryOperations.OverlapOperator(info1, info2);
                }
            }

            // Visualize the identified elements
            control.Visualize(control.CreateModelUiElementsDs(model, doorOpenings.Select(item => item.GlobalId).Distinct().ToList(), false));
            
            // Get the doors

            // meshBuilder.AddPolygon(points);
            // 
            // 
            // var material = new DiffuseMaterial { Brush = Brushes.Red };
            // var myGeometryModel = new GeometryModel3D
            // {
            //     Material = material,
            //     BackMaterial = material,
            //     Geometry = meshBuilder.ToMesh(true)
            // };
            // 
            // var element = new ModelUIElement3D { Model = myGeometryModel };
            // 
            // control.Viewport3D.Children.Add(element);

        }

        public override Node Clone()
        {
            return new IfcAccessibilityGraph(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}
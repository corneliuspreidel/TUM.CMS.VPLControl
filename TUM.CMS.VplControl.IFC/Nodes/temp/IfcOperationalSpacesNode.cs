using System.Collections.Generic;
using System.Linq;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.Utilities;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;
using Xbim.ModelGeometry.Scene;
using Brushes = System.Windows.Media.Brushes;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcOperationalSpacesNode : Node
    {
        public IfcStore xModel;
        private Xbim3DModelContext context;

        private ModelController modelController;

        private IFCViewerControl control;

        public IfcOperationalSpacesNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            AddInputPortToNode("ModelInfo", typeof(object));

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

            // CREATE THE OPERATIONAL SPACE FOR DOORS
            var doors = xModel.Instances.OfType<IIfcDoor>();
            var elementIds = new List<IfcGloballyUniqueId>();
            foreach (var item in doors)
            {
                elementIds.Add(item.GlobalId);
            }

            // control.InitViewPort();
            control.Visualize(control.CreateModelUiElementsDs(model, elementIds, false));

            foreach (var item in doors)
            {
                MeshBuilder meshBuilder = new MeshBuilder(false, false);


                // if (((IIfcDoorStyle)item.IsTypedBy.First().RelatingType).OperationType == IfcDoorStyleOperationEnum.DOUBLE_DOOR_SINGLE_SWING)
                // {

                var placement = ((IfcLocalPlacement)item.ObjectPlacement).PlacementRelTo.ReferencedByPlacements.First();
                var mat = placement.ToMatrix3D();

                // Set the origin
                var x = mat.OffsetX;
                var y = mat.OffsetY;
                var z = mat.OffsetZ;

                var locPoint = new Point3D(x, y, z);
                var allignVector = new Vector3D();
                var normalVector = new Vector3D();
                var heightVector = new Vector3D(0, 0, (double) item.OverallHeight);

                foreach (XbimShapeInstance instance in context.ShapeInstancesOf(item).Where(i =>i.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded))
                {
                    allignVector = new Vector3D((double) (instance.Transformation.Left.X * item.OverallWidth), (double)(instance.Transformation.Left.Y * item.OverallWidth), (double)(instance.Transformation.Left.Z * item.OverallWidth));
                    normalVector = Vector3D.CrossProduct(allignVector, new Vector3D(0, 0, 1));
                }
                // allignVector.Negate();
                // Base Area
                // var listpoints = new List<Point>
                // {
                //     new Point(locPoint.X, locPoint.Y),
                //     new Point(locPoint.X + normalVector.X, locPoint.Y + normalVector.Y),
                //     new Point(locPoint.X + normalVector.X - allignVector.X, locPoint.Y + normalVector.Y - allignVector.Y),
                //     new Point(locPoint.X - allignVector.X, locPoint.Y - allignVector.Y)
                // };

                // var rec = new Rect3D(locPoint.X, locPoint.Y, locPoint.Z, Math.Abs(heightVector.X + allignVector.X + normalVector.X), Math.Abs(heightVector.Y + allignVector.Y + normalVector.Y), Math.Abs(heightVector.Z + allignVector.Z + normalVector.Z));
                // 
                // meshBuilder.AddBox(rec);

                // var profile = new[] { new Point(0, 0.4), new Point(0.06, 0.36), new Point(0.1, 0.1), new Point(0.34, 0.1), new Point(0.4, 0.14), new Point(0.5, 0.5), new Point(0.7, 0.56), new Point(1, 0.46) };
                // meshBuilder.AddRevolvedGeometry(listpoints, null, locPoint, heightVector, 10);

                // meshBuilder.AddBox(new Rect3D(locPoint.X, locPoint.Y, locPoint.Z, Math.Abs(normalVector.X + allignVector.X), Math.Abs(normalVector.Y+ allignVector.Y), Math.Abs(normalVector.Z + allignVector.Z)));
                //meshBuilder.AddExtrudedGeometry(listpoints, new Vector3D(1,1,1), locPoint, Point3D.Add(locPoint, heightVector));
                // meshBuilder.AddExtrudedSegments(listpoints, new Vector3D(), locPoint, Point3D.Add(locPoint, heightVector));

                var listpoints3D = new List<Point3D>
                {
                    // 2D Rectangle
                    new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X , locPoint.Y + normalVector.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X - allignVector.X, locPoint.Y + normalVector.Y - allignVector.Y, locPoint.Z),
                    new Point3D(locPoint.X - allignVector.X, locPoint.Y - allignVector.Y, locPoint.Z),
                
                    // 3D BOX
                    // new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    // new Point3D(locPoint.X + normalVector.X , locPoint.Y + normalVector.Y, locPoint.Z),
                    // new Point3D(locPoint.X + normalVector.X - allignVector.X, locPoint.Y + normalVector.Y - allignVector.Y, locPoint.Z),
                    // new Point3D(locPoint.X - allignVector.X, locPoint.Y - allignVector.Y, locPoint.Z),
                    // new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    // new Point3D(locPoint.X, locPoint.Y, locPoint.Z + heightVector.Z),
                    // new Point3D(locPoint.X + normalVector.X , locPoint.Y + normalVector.Y, locPoint.Z + heightVector.Z),
                    // new Point3D(locPoint.X + normalVector.X - allignVector.X, locPoint.Y + normalVector.Y - allignVector.Y, locPoint.Z + heightVector.Z),
                    // new Point3D(locPoint.X - allignVector.X, locPoint.Y - allignVector.Y, locPoint.Z + heightVector.Z),
                };

                meshBuilder.AddPolygon(listpoints3D);

                // meshBuilder.AddBox(new Point3D((locPoint.X + normalVector.X - allignVector.X) / 2 , (locPoint.Y + normalVector.Y - allignVector.Y) / 2, (locPoint.Z + normalVector.Z - allignVector.Z + heightVector.Z) / 2), normalVector.X - allignVector.X, normalVector.Y - allignVector.Y, heightVector.Z);

                //meshBuilder.AddPolygonByCuttingEars();

                //meshBuilder.AddLoftedGeometry(new List<IList<Point3D>> { listpoints3D}, new List<IList<Vector3D>> {new List<Vector3D> {heightVector}});
                // meshBuilder.AddBox();

                // Width Vector
                // meshBuilder.AddArrow(new Point3D(x, y, z), new Point3D((double) ((x + mat.Right.X) * item.OverallWidth), y + mat.Right.Y, z + mat.Right.Z), 0.01);
                // meshBuilder.AddArrow(new Point3D(x, y, z), new Point3D((double)((x + mat.Right.X) * item.OverallWidth), y + mat.Right.Y, z + mat.Right.Z), 0.01);

                // var vecHeight = new Vector3D(0, 0, (double) (z + item.OverallHeight) - z);
                // var vecFront = new Vector3D(mat.Forward.X, mat.Forward.Y, 0);
                // var vecWidth = Vector3D.CrossProduct(vecHeight, vecFront);

                var material = new DiffuseMaterial { Brush = Brushes.Aqua };
                var myGeometryModel = new GeometryModel3D
                {
                    Material = material,
                    BackMaterial = material,
                    Geometry = meshBuilder.ToMesh(true)
                };

                var element = new ModelUIElement3D { Model = myGeometryModel };

                control.Viewport3D.Children.Add(element);

                // }
            }

            // Create the floorplan
            // Create FloorPlan
            var walls = xModel.Instances.OfType<IIfcWall>();
            foreach (var wall in walls)
            {
                MeshBuilder meshBuilder = new MeshBuilder(false, false);

                //var points = GeometryHandler.DeriveGeometry2D(wall, context);
                var points = GeometryHandler.GetFootPrint(wall, context);

                meshBuilder.AddPolygon(points);


                var material = new DiffuseMaterial { Brush = Brushes.Red };
                var myGeometryModel = new GeometryModel3D
                {
                    Material = material,
                    BackMaterial = material,
                    Geometry = meshBuilder.ToMesh(true)
                };

                var element = new ModelUIElement3D { Model = myGeometryModel };

                control.Viewport3D.Children.Add(element);

            }

        }

        public override Node Clone()
        {
            return new IfcOperationalSpacesNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}
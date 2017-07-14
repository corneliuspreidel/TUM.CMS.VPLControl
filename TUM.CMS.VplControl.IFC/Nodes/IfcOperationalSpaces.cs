using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
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
using Xbim.Ifc2x3.GeometricModelResource;
using Xbim.Ifc2x3.GeometryResource;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.ProfileResource;
using Xbim.Ifc2x3.RepresentationResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;
using Xbim.IO.Step21;
using Xbim.ModelGeometry.Scene;
using Brushes = System.Windows.Media.Brushes;
using IfcProfileTypeEnum = Xbim.Ifc4.Interfaces.IfcProfileTypeEnum;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcOperationalSpaces : Node
    {
        public IfcStore xModel;
        private Xbim3DModelContext context;

        private ModelController modelController;

        private IfcOperationalSpacesControl _control;

        public IfcOperationalSpaces(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            IsResizeable = true;

            AddInputPortToNode("ModelInfo", typeof(ModelInfo));

            modelController = ModelController.Instance;

            _control = new IfcOperationalSpacesControl();
            AddControlToNode(_control);
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            var modelInfo = InputPorts[0].Data as ModelInfo;

            if (_control.FrontWidth.Text == "" || _control.FrontWidth.Text == "" || _control.FrontWidth.Text == "" || _control.FrontWidth.Text == "")
            {
                _control.settingsExpander.Header = "Please Check Settings";
                return;
            }
            else
            {
                _control.settingsExpander.Header = "Settings";
            }

            var frontWidth = _control.FrontWidth.Text.ToDouble();
            var frontDepth = _control.FrontDepth.Text.ToDouble();
            var backWidth = _control.BackWidth.Text.ToDouble();
            var backDepth = _control.BackDepth.Text.ToDouble();

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
            _control.viewer.Visualize(_control.viewer.CreateModelUiElementsDs(model, elementIds, false));

            foreach (var item in doors)
            {
                MeshBuilder meshBuilder = new MeshBuilder(false, false);

                var placement = ((IfcLocalPlacement) item.ObjectPlacement).PlacementRelTo.ReferencedByPlacements.First();
                var mat = placement.ToMatrix3D();

                // Set the origin
                var x = mat.OffsetX;
                var y = mat.OffsetY;
                var z = mat.OffsetZ;

                var locPoint = new Point3D(x, y, z);
                var allignVector = new Vector3D();
                var normalVector = new Vector3D();
                // var heightVector = new Vector3D(0, 0, (double) item.OverallHeight);

                var doorWidth = item.OverallWidth;

                foreach (
                    XbimShapeInstance instance in
                        context.ShapeInstancesOf(item)
                            .Where(
                                i => i.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded)
                    )
                {
                    allignVector = new Vector3D((instance.Transformation.Left.X), (instance.Transformation.Left.Y),
                        (instance.Transformation.Left.Z));
                    normalVector = Vector3D.CrossProduct(allignVector, new Vector3D(0, 0, 1));
                }

                // Create the operational spaces
                var operationalSpaces = new List<List<Point3D>>();


                // FRONT
                var listpoints3D = new List<Point3D>
                {
                    // 2D Rectangle
                    new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*frontDepth, locPoint.Y + normalVector.Y*frontDepth,
                        locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*frontDepth - allignVector.X*frontWidth,
                        locPoint.Y + normalVector.Y*frontDepth - allignVector.Y*frontWidth, locPoint.Z),
                    new Point3D(locPoint.X - allignVector.X*frontWidth, locPoint.Y - allignVector.Y*frontWidth,
                        locPoint.Z),
                };
                meshBuilder.AddPolygon(listpoints3D);
                operationalSpaces.Add(listpoints3D);

                listpoints3D = new List<Point3D>
                {
                    // 2D Rectangle
                    new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*backDepth, locPoint.Y + normalVector.Y*backDepth, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*backDepth - allignVector.X*backWidth,
                        locPoint.Y + normalVector.Y*backDepth - allignVector.Y*backWidth, locPoint.Z),
                    new Point3D(locPoint.X - allignVector.X*backWidth, locPoint.Y - allignVector.Y*backWidth, locPoint.Z),
                };
                meshBuilder.AddPolygon(listpoints3D);
                operationalSpaces.Add(listpoints3D);

                var material = new DiffuseMaterial {Brush = Brushes.Red};
                var myGeometryModel = new GeometryModel3D
                {
                    Material = material,
                    BackMaterial = material,
                    Geometry = meshBuilder.ToMesh(true)
                };

                var element = new ModelUIElement3D {Model = myGeometryModel};

                _control.viewer.Viewport3D.Children.Add(element);
                // BACK
                meshBuilder = new MeshBuilder();
                // Negate the normal Vector! 
                normalVector.Negate();

                listpoints3D = new List<Point3D>
                {
                    // 2D Rectangle
                    new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*frontDepth, locPoint.Y + normalVector.Y*frontDepth,
                        locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*frontDepth - allignVector.X*frontWidth,
                        locPoint.Y + normalVector.Y*frontDepth - allignVector.Y*frontWidth, locPoint.Z),
                    new Point3D(locPoint.X - allignVector.X*frontWidth, locPoint.Y - allignVector.Y*frontWidth,
                        locPoint.Z),
                };
                meshBuilder.AddPolygon(listpoints3D);
                operationalSpaces.Add(listpoints3D);

                listpoints3D = new List<Point3D>
                {
                    // 2D Rectangle
                    new Point3D(locPoint.X, locPoint.Y, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*backDepth, locPoint.Y + normalVector.Y*backDepth, locPoint.Z),
                    new Point3D(locPoint.X + normalVector.X*backDepth - allignVector.X*backWidth,
                        locPoint.Y + normalVector.Y*backDepth - allignVector.Y*backWidth, locPoint.Z),
                    new Point3D(locPoint.X - allignVector.X*backWidth, locPoint.Y - allignVector.Y*backWidth, locPoint.Z),
                };
                meshBuilder.AddPolygon(listpoints3D);
                operationalSpaces.Add(listpoints3D);

                material = new DiffuseMaterial {Brush = Brushes.Aqua};
                myGeometryModel = new GeometryModel3D
                {
                    Material = material,
                    BackMaterial = material,
                    Geometry = meshBuilder.ToMesh(true)
                };

                element = new ModelUIElement3D {Model = myGeometryModel};

                _control.viewer.Viewport3D.Children.Add(element);

                // Create the Spaces directly in the IFC File 
                // Get the building
                var building = xModel.Instances.OfType<IIfcBuilding>();

                if (building == null)
                    return;

                foreach (var oSpace in operationalSpaces)
                {
                    var space = CreateSpace(xModel, 100, 100, 100);
                    using (var txn = xModel.BeginTransaction("Add Space"))
                    {

                        (building.FirstOrDefault() as IfcBuilding).AddElement(space);
                        txn.Commit();
                    }
                }

            }

            // Create the floorplan
            // Create FloorPlan
            // var walls = xModel.Instances.OfType<IIfcWall>();
            // foreach (var wall in walls)
            // {
            //     MeshBuilder meshBuilder = new MeshBuilder(false, false);
            // 
            //     var points = GeometryHandler.GetFootPrint(wall, context);
            // 
            //     meshBuilder.AddPolygon(points);
            // 
            //     var material = new DiffuseMaterial {Brush = Brushes.Red};
            //     var myGeometryModel = new GeometryModel3D
            //     {
            //         Material = material,
            //         BackMaterial = material,
            //         Geometry = meshBuilder.ToMesh(true)
            //     };
            // 
            //     var element = new ModelUIElement3D {Model = myGeometryModel};
            // 
            //     _control.viewer.Viewport3D.Children.Add(element);
            // 
            // }

            foreach (var storey in xModel.Instances.OfType<IIfcBuildingStorey>())
            {
                var spaces = storey.Spaces;
                foreach (var space in spaces)
                {
                    MeshBuilder meshBuilder = new MeshBuilder(false, false);

                    var points = GeometryHandler.GetFootPrintNonBREP(space, context);
                    // var points = GeometryHandler.GetFootPrint(space, context);
                    meshBuilder.AddPolygon(points);

                    var brush = new SolidColorBrush(Colors.Blue) { Opacity = 0.3 };

                    var material = new DiffuseMaterial { Brush = brush };
                    var myGeometryModel = new GeometryModel3D
                    {
                        Material = material,
                        BackMaterial = material,
                        Geometry = meshBuilder.ToMesh(true)
                    };

                    var element = new ModelUIElement3D { Model = myGeometryModel };
                    // element.Model.Transform.Transform(new Point3D(0, 0, 1.1));

                    _control.viewer.Viewport3D.Children.Add(element);

                }
            }
        }

        public override Node Clone()
        {
            return new IfcOperationalSpaces(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        static private IfcSpace CreateSpace(IfcStore model, double length, double width, double height)
        {
            //
            //begin a transaction
            using (var txn = model.BeginTransaction("Create Space"))
            {
                var space = model.Instances.New<IfcSpace>();
                space.Name = "Space";

                //represent wall as a rectangular profile
                var rectProf = model.Instances.New<IfcRectangleProfileDef>();
                rectProf.ProfileType = (Xbim.Ifc2x3.ProfileResource.IfcProfileTypeEnum) IfcProfileTypeEnum.AREA;
                rectProf.XDim = width;
                rectProf.YDim = length;

                var insertPoint = model.Instances.New<IfcCartesianPoint>();
                insertPoint.SetXY(0, 400); //insert at arbitrary position
                rectProf.Position = model.Instances.New<IfcAxis2Placement2D>();
                rectProf.Position.Location = insertPoint;

                //model as a swept area solid
                var body = model.Instances.New<IfcExtrudedAreaSolid>();
                body.Depth = height;
                body.SweptArea = rectProf;
                body.ExtrudedDirection = model.Instances.New<IfcDirection>();
                body.ExtrudedDirection.SetXYZ(0, 0, 1);

                //parameters to insert the geometry in the model
                var origin = model.Instances.New<IfcCartesianPoint>();
                origin.SetXYZ(0, 0, 0);
                body.Position = model.Instances.New<IfcAxis2Placement3D>();
                body.Position.Location = origin;

                //Create a Definition shape to hold the geometry
                var shape = model.Instances.New<IfcShapeRepresentation>();
                var modelContext = model.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
                shape.ContextOfItems = modelContext;
                shape.RepresentationType = "SweptSolid";
                shape.RepresentationIdentifier = "Body";
                shape.Items.Add(body);

                //Create a Product Definition and add the model geometry to the wall
                var rep = model.Instances.New<IfcProductDefinitionShape>();
                rep.Representations.Add(shape);
                space.Representation = rep;

                //now place the wall into the model
                var lp = model.Instances.New<IfcLocalPlacement>();
                var ax3D = model.Instances.New<IfcAxis2Placement3D>();
                ax3D.Location = origin;
                ax3D.RefDirection = model.Instances.New<IfcDirection>();
                ax3D.RefDirection.SetXYZ(0, 1, 0);
                ax3D.Axis = model.Instances.New<IfcDirection>();
                ax3D.Axis.SetXYZ(0, 0, 1);
                lp.RelativePlacement = ax3D;
                space.ObjectPlacement = lp;

                return space;
            }
        }
    }
}
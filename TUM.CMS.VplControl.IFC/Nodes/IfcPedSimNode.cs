using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MIConvexHull;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using ClipperLib;
using Xbim.Ifc;

namespace TUM.CMS.VplControl.IFC.Nodes
{
    public class IfcPedSimNode: Node
    {
        private ModelController modelController;
        private Xbim3DModelContext context;
        private IfcStore xModel;

        private PedSimNodeControl pedSimNodeControl;
        private simulator sim;

        private double MinX;
        private double MaxX;
        private double MinY;
        private double MaxY;

        public IfcPedSimNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            modelController = ModelController.Instance;
            AddInputPortToNode("ModelInfo", typeof(ModelInfo));

            pedSimNodeControl = new PedSimNodeControl();
            AddControlToNode(pedSimNodeControl);
            pedSimNodeControl.storeyComboBox.SelectionChanged += ComboBoxOnSelectionChanged;
            IsResizeable = true;

            pedSimNodeControl.viewer.ViewPort3D.MouseDown += ViewPort3DOnMouseDown;
        }

        private void ViewPort3DOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point location = e.GetPosition(pedSimNodeControl.viewer.ViewPort3D);
            ModelUIElement3D result = GetHitResult(location);
            if (result == null)
            {
                return;
            }
            //Do Stuff Here
        }

        ModelUIElement3D GetHitResult(Point location)
        {
            if (sim == null)
                InitSimulation();

            HitTestResult result = VisualTreeHelper.HitTest(pedSimNodeControl.viewer.ViewPort3D, location);

            if (result != null & result.GetType() == typeof(RayMeshGeometry3DHitTestResult))
            {
                var pointHit = ((RayMeshGeometry3DHitTestResult)result).PointHit;

                // Create Rectangle
                var points = new List<point>()
                {
                    new point() {x = pointHit.X, y = pointHit.Z},
                    new point() {x = pointHit.X + 1, y = pointHit.Z},
                    new point() {x = pointHit.X + 1, y = pointHit.Z + 1},
                    new point() {x = pointHit.X, y = pointHit.Z + 1}
                };

                sim.layouts[0].scenario.AddArea("OriginArea", points, PedSimXMLWriter.areaType.Origin);

                // SERIALIZE
                SerializeSimulation();

            }

            if (result != null && result.VisualHit is ModelUIElement3D)
            {
                ModelUIElement3D visual = (ModelUIElement3D)result.VisualHit;
                return visual;
            }

            return null;
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
            if (storeys.Count() == 0)
                return;

            DataContext = this;

            pedSimNodeControl.storeyComboBox.ItemsSource = storeys;
            pedSimNodeControl.storeyComboBox.DisplayMemberPath = "FriendlyName";

            // INIT THE SIMULATION
            InitSimulation();

        }

        private void InitSimulation()
        {
            // INIT THE SIMULATION
            sim = new simulator
            {
                simEnd = 0.0,
                layouts = new List<layouts>()
            };
            sim.layouts.Add(new layouts());
            sim.layouts[0].scenario = new scenario
            {
                id = 0,
                name = "IFC_Test"
            };
        }

        private void ComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            // INIT THE SIMULATION
            InitSimulation();

            // INTERMEDIATE AREAS FROM ROOMS/SPACES
            var storey = (sender as ComboBox).SelectedItem as IfcBuildingStorey;
            foreach (var space in storey.Spaces)
            {
                // var points = DeriveGeometry(space);
                // var points = DeriveGeometry2D(space);

                var points = CastPoints(GeometryHandler.GetFootPrint(space, context));

                if (points == null)
                    continue;

                sim.layouts[0].scenario.AddArea(space.Name, points, PedSimXMLWriter.areaType.Intermediate);
            }

            // INTERMEDIATE AREAS FROM DOORS
            List<List<point>> doorOpeningElements = new List<List<point>>();

            var doors = xModel.Instances.OfType<IfcRelFillsElement>();
            foreach (var item in doors)
            {
                if (item.RelatedBuildingElement.GetType().ToString().Contains("Door"))
                {
                    var points = CastPoints(GeometryHandler.GetFootPrint(item.RelatingOpeningElement, context));
                    // var points = DeriveGeometry2D(item.RelatingOpeningElement);
                    if (points == null)
                        continue;
                    doorOpeningElements.Add(points);
                    sim.layouts[0].scenario.AddArea(item.Name, points, PedSimXMLWriter.areaType.Intermediate);
                }
            }

            // COMPUTE THE ORIGIN AREAS FOR EACH ROOM/SPACE
            // foreach (var space in storey.Spaces)
            // {
            //     var points = DeriveGeometry2D(space);
            // }

            // Get the openings ... 
            // List<IfcProduct> doors = new List<IfcProduct>();
            // List<List<point>> doorPolygons = new List<List<point>>();
            // foreach (var item in storey.ContainsElements.ElementAt(0).RelatedElements)
            // {
            //     if (item.GetType().ToString().Contains(@"Door"))
            //     {
            //         // var relVoids = xModel.Instances.OfType<IfcRelFillsElement>();
            //         // foreach (var relVoid in relVoids)
            //         // {
            //         //     if (relVoid.RelatedBuildingElement == item)
            //         //     {
            //         //         var points = DeriveGeometry(relVoid.RelatingOpeningElement);
            //         //         sim.layouts[0].scenario.AddArea(relVoid.RelatingOpeningElement.Name, points, PedSimXMLWriter.areaType.Intermediate);
            //         //     }
            //         // }
            //         // (item as IfcDoor).FillsVoids.
            //         doors.Add(item);
            //     }
            // }
            // foreach (var door in doors)
            // {
            //     var points = DeriveGeometry(door);
            //     doorPolygons.Add(points);
            //     sim.layouts[0].scenario.AddArea(door.Name, points, PedSimXMLWriter.areaType.Intermediate);
            // }

            // GET THE COLUMNS AS OBSTACLES
            // List<IfcProduct> columns = new List<IfcProduct>();
            // 
            // foreach (var item in storey.ContainsElements.ElementAt(0).RelatedElements)
            // {
            //     if (item.GetType().ToString().Contains("Column"))
            //     {
            //         columns.Add(item);
            //     }
            // }

            // foreach (var column in columns)
            // {
            //     var columnPoints = DeriveGeometry2D(column);
            //     sim.layouts[0].scenario.AddSolidObstacle(column.Name, columnPoints);
            // }

            // GET THE WALLS AS OBSTACLES
            List<IfcProduct> walls = new List<IfcProduct>();
            
            foreach (var item in storey.ContainsElements.ElementAt(0).RelatedElements)
            {
                if (item.GetType().ToString().Contains("Wall"))
                {
                    walls.Add(item);
                }
            }
            
            foreach (var wall in walls)
            {
                // var wallPoints = DeriveGeometry2D(wall);
                var wallPoints = CastPoints(GeometryHandler.GetFootPrint(wall, context));
                // Clipping - Walls are clipped by openings
                int multiplyFactor = 90;
                
                // WallPolygon
                List<List<IntPoint>> subj = new List<List<IntPoint>>();
                subj.Add(new List<IntPoint>());
                
                foreach (var item in wallPoints)
                {
                    subj[0].Add(new IntPoint((long)(item.x * multiplyFactor), (long) (item.y * multiplyFactor)));
                }
                // subj[0].Distinct();
                
                // Clipping Opening Area from doors
                List<List<IntPoint>> clip = new List<List<IntPoint>>();
                int i = 0;
                foreach (var opening in doorOpeningElements)
                { 
                    clip.Add(new List<IntPoint>());
                    foreach (var item in opening)
                    {
                        clip[i].Add(new IntPoint((long)(item.x * multiplyFactor), (long)(item.y * multiplyFactor)));
                    }
                    i++;
                }
                
                var solution = new List<List<IntPoint>>();
                var c = new Clipper();
                // c.ReverseSolution = true; 
                
                c.AddPolygons(subj, PolyType.ptSubject);
                c.AddPolygons(clip, PolyType.ptClip);
                
                c.Execute(ClipType.ctDifference, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                
                foreach (var solutionItem in solution)
                {
                                       List<point> sol = new List<point>();
                    foreach (var coord in  solutionItem)
                    {
                        sol.Add(new point { x = ((double)coord.X) / multiplyFactor, y = ((double)coord.Y) / multiplyFactor });
                    }
                
                    var convexSol = CreateConvexHull(sol);
                
                    sim.layouts[0].scenario.AddSolidObstacle(wall.Name, convexSol);
                }
                if (solution.Count == 0)
                {
                    sim.layouts[0].scenario.AddSolidObstacle(wall.Name, wallPoints);
                }
            }

            // SET MIN & MAX
            sim.layouts[0].scenario.maxX = MaxX;
            sim.layouts[0].scenario.minX = MinX;
            sim.layouts[0].scenario.maxY = MaxY;
            sim.layouts[0].scenario.minY = MinY;

            // SERIALIZE
            SerializeSimulation();
        }

        public void SerializeSimulation()
        {
            // WRITE THESIMULATION TO XML
            string filePath = @"D:\dev\TUM.CMS.VPLControl\momenTUMv2\INPUT.xml";
            PedSimXMLWriter.Serialize(sim, filePath);
            pedSimNodeControl.viewer.ShowModel(filePath);
        }

        public List<point> CastPoints(List<Point3D> pointList)
        {
            if (pointList == null)
                return null; 

            var list = new List<point>();

            foreach (var item in pointList)
            {
                list.Add(new point() {x= item.X, y = item.Y});
            }
            return list;
        }

        // public List<point> DeriveGeometry2D(IIfcProduct ifcElement)
        // {
        //     var resultList = new List<point>();
        // 
        //     // var allTriangles = new List<Triangles>();
        //     // foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement).Where(x => x.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded))
        //     foreach (XbimShapeInstance instance in context.ShapeInstancesOf(ifcElement))
        //     {
        //         XbimShapeGeometry geometry = context.ShapeGeometry(instance);
        // 
        //         var data = ((IXbimShapeGeometryData)geometry).ShapeData;
        //         using (var stream = new MemoryStream(data))
        //         {
        //             using (var reader = new BinaryReader(stream))
        //             {
        //                 XbimShapeTriangulation mesh = reader.ReadShapeTriangulation();
        //                 mesh = mesh.Transform(instance.Transformation);
        // 
        //                 // find the minimal z coordinate
        //                 double minZ = 10000;
        //                 foreach (var vertex in mesh.Vertices)
        //                 {
        //                     if (vertex.Z <= minZ)
        //                         minZ = vertex.Z;
        //                 }
        //                 List<IVertex> points = new List<IVertex>();
        // 
        //                 foreach (var vertex in mesh.Vertices)
        //                 {
        //                     if (vertex.Z != minZ)
        //                         continue;
        // 
        //                     points.Add(new DefaultVertex { Position = new[] { vertex.X, vertex.Y } });
        // 
        //                     if (vertex.X >= MaxX)
        //                         MaxX = vertex.X;
        //                     if (vertex.Y >= MaxY)
        //                         MaxY= vertex.Y;
        // 
        //                     if (vertex.X <= MinX)
        //                         MinX = vertex.X;
        //                     if (vertex.Y <= MinY)
        //                         MinY = vertex.Y;
        //                 }
        // 
        //                 if (points.Count <= 2)
        //                     return null;
        //                 
        //                 // Compute ConvexHull
        //                 var cH = ConvexHull.Create(points);
        // 
        //                 foreach (var item in cH.Points)
        //                 {
        //                     var point = new point() { x = item.Position[0], y = item.Position[1] };
        //                     bool duplicate = false;
        //                     foreach (var result in resultList)
        //                     {
        //                         if (result == point)
        //                         {
        //                             duplicate = true;
        //                             break;
        //                         }
        //                     }
        //                     if(!duplicate)
        //                         resultList.Add(point);
        //                 }
        // 
        //             }
        //         }
        //     }
        //     return resultList;
        // }

        /// <summary>
        /// Create a Convex Hull for a list of 2D-points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public List<point> CreateConvexHull(List<point> inputPoints)
        {
            List<IVertex> points = new List<IVertex>();
        
            foreach (var point in inputPoints)
            {
                points.Add(new DefaultVertex { Position = new[] { point.x, point.y} });
            }
        
            if (points.Count <= 2)
                return null;
        
            // Compute ConvexHull
            var cH = ConvexHull.Create(points);

            var result = new List<point>();
            foreach (var item in cH.Points)
            {
                result.Add(new point {x= item.Position[0], y = item.Position[1] });
            }
            return result;
        }

        public override Node Clone()
        {
            return new IfcPedSimNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}

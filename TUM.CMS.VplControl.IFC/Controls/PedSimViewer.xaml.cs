using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.ExtendedVplControl.Controls;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Controls
{
    /// <summary>
    /// Interaction logic for PedSimViewer.xaml
    /// </summary>
    public partial class PedSimViewer : UserControl
    {
        private ModelUIElement3D graphModel;

        public PedSimViewer()
        {
            InitializeComponent();
        }

        public void ShowModel(string filePath)
        {
            // Empty the viewer
            ViewPort3D.Children.Clear();
            // Init Lights
            var lights = new SunLight();
            ViewPort3D.Children.Add(lights);

            var sim = PedSimXMLWriter.Deserialize(filePath);
            if (sim == null)
                return;

            MeshBuilder mb;
            // Visualize Areas
            foreach (var item in sim.layouts[0].scenario.areas)
            {
                mb = new MeshBuilder(false, false);

                List<Point3D> ptd3Ds = new List<Point3D>();
                var material = new DiffuseMaterial();

                if (item.type == PedSimXMLWriter.areaType.Origin.ToString())
                {
                    foreach (var point in item.points)
                    {
                        ptd3Ds.Add(new Point3D {X = point.x, Y = point.y, Z = 0.000001});
                    }

                    mb.AddPolygon(ptd3Ds);
                    material = new DiffuseMaterial {Brush = Brushes.Chartreuse};
                }
                else if (item.type == PedSimXMLWriter.areaType.Intermediate.ToString())
                {
                    foreach (var point in item.points)
                    {
                        ptd3Ds.Add(new Point3D {X = point.x, Y = point.y, Z = 0});
                    }
                    mb.AddPolygon(ptd3Ds);
                    material = new DiffuseMaterial {Brush = Brushes.Aqua};
                }

                var myGeometryModel = new GeometryModel3D
                {
                    Material = material,
                    BackMaterial = material,
                    Geometry = mb.ToMesh(true)

                };

                ViewPort3D.Children.Add(new ModelUIElement3D {Model = myGeometryModel});
            }

            // Visualize Obstacles
            foreach (var item in sim.layouts[0].scenario.obstacles)
            {
                mb = new MeshBuilder(false, false);

                List<Point3D> ptd3Ds = new List<Point3D>();

                foreach (var point in item.points)
                {
                    ptd3Ds.Add(new Point3D() {X = point.x, Y = point.y, Z = 0});
                }

                mb.AddPolygon(ptd3Ds);

                var myGeometryModel = new GeometryModel3D
                {
                    Material = new DiffuseMaterial() {Brush = Brushes.Red},
                    BackMaterial = new DiffuseMaterial() {Brush = Brushes.Red},
                    Geometry = mb.ToMesh(true)
                };

                ViewPort3D.Children.Add(new ModelUIElement3D {Model = myGeometryModel});
            }

        }

        public void ShowGraphModel(string filePath)
        {

            var sim = PedSimXMLWriter.Deserialize(filePath);
            if (sim == null)
                return;

            MeshBuilder mb = new MeshBuilder(false, false);

            foreach (var item in sim.layouts[0].scenario.graphs[0].vertices)
            {
                mb.AddSphere(new Point3D(item.center.x, item.center.y, 0), 0.1);
            }

            foreach (var item in sim.layouts[0].scenario.graphs[0].edges)
            {
                var vertexLeft = new vertex();
                foreach (var vertex in sim.layouts[0].scenario.graphs[0].vertices)
                {
                    if (vertex.id == item.idLeft)
                    {
                        vertexLeft = vertex;
                        break;
                    }
                }

                var vertexRight = new vertex();
                foreach (var vertex in sim.layouts[0].scenario.graphs[0].vertices)
                {
                    if (vertex.id == item.idRight)
                    {
                        vertexRight = vertex;
                        break;
                    }
                }

                // Point new Point3D(item.center.x, item.center.y, 0)
                if (vertexRight != null && vertexLeft != null)
                    mb.AddArrow(new Point3D(vertexLeft.center.x, vertexLeft.center.y, 0),
                        new Point3D(vertexRight.center.x, vertexRight.center.y, 0), 0.05, 0);
                else
                {
                    var test = true;
                }

            }

            var myGeometryModel = new GeometryModel3D
            {
                Material = new DiffuseMaterial() {Brush = Brushes.Green},
                BackMaterial = new DiffuseMaterial() {Brush = Brushes.Green},
                Geometry = mb.ToMesh(true)
            };

            graphModel = new ModelUIElement3D {Model = myGeometryModel};
            ViewPort3D.Children.Add(graphModel);
        }

        public void RemoveGraphModel()
        {
            if (graphModel != null)
            {
                try
                {
                    ViewPort3D.Children.Remove(graphModel);
                }
                catch (Exception e)
                {
                   Console.WriteLine(e.Message);
                }
            }
                
        }
    }
}

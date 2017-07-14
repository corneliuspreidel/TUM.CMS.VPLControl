using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BimPlus.Sdk.Data.DbCore;
using BimPlus.Sdk.Data.Geometry;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Watch3D.Nodes;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class BimPlusWatch3DNode : Watch3DNode
    {
        // Data Source Connection
        private readonly DataController _controller;

        // BackgroundWorker
        public BackgroundWorker Worker;

        // Geometry Members
        protected List<MeshIdandGeometry> MyGeometry { get; set; }
        protected List<MeshIdandGeometry> _fallBackGeometry { get; set; }

        private ModelInfo modelInfo;

        private ContainerUIElement3D _container;

        public BimPlusWatch3DNode(Core.VplControl hostCanvas): base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            HelixViewport3D.Title = "BimPlus Viewer";

            // Refresh the selected Models
            SelectedModels = new List<GeometryModel3D>();
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data != null)
            {
                Worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                Worker.DoWork += WorkerOnDoWork;
                Worker.ProgressChanged += WorkerOnProgressChanged;
                Worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
                Worker.RunWorkerAsync(10000);
            }
            else
            {   
                base.Calculate();
            }
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            // Implement progressBar
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                ProgressBar.Value = progressChangedEventArgs.ProgressPercentage;
            });
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (runWorkerCompletedEventArgs.Result == null)
            {
                Dispatcher.BeginInvoke((Action)delegate ()
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressLabel.Visibility = Visibility.Collapsed;
                });
                return;
            }

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressLabel.Visibility = Visibility.Collapsed;
            });

            _container = new ContainerUIElement3D();

            // BimPlusData
            if (runWorkerCompletedEventArgs.Result.GetType() == typeof(List<MeshIdandGeometry>))
            {
                var geometry = runWorkerCompletedEventArgs.Result as List<MeshIdandGeometry>;

                // Set it global 
                MyGeometry = geometry;
                _fallBackGeometry = new List<MeshIdandGeometry>();

                if (MyGeometry != null)
                    foreach (var mygeometry in MyGeometry)
                    {
                        // var newgeometry = mygeometry.Model3D;
                        var newgeometry = mygeometry.Model3D.Clone();
                        _fallBackGeometry.Add(new MeshIdandGeometry {Id = mygeometry.Id, Material = mygeometry.Material, Model3D = newgeometry});
                        var element = new ModelUIElement3D { Model = newgeometry };
                        element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this);
                        _container.Children.Add(element);
                    }

                // First of all clear the view 
                HelixViewport3D.Children.Clear();
                HelixViewport3D.Children.Add(_container);
                HelixViewport3D.CameraController.ZoomExtents();
                HelixViewport3D.Children.Add(new SunLight());
                HelixViewport3D.Children.Add(new DefaultLights());
            }
            // Other File Data
            else if (runWorkerCompletedEventArgs.Result is ModelVisual3D)
            {
                var model = runWorkerCompletedEventArgs.Result as ModelVisual3D;

                HelixViewport3D.Children.Clear();
                HelixViewport3D.Children.Add(model);
                HelixViewport3D.CameraController.ZoomExtents();
                HelixViewport3D.Children.Add(new DefaultLights());
            }
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            if (InputPorts[0].Data == null)
                return;

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                ProgressBar.Visibility = Visibility.Visible;
                ProgressLabel.Visibility = Visibility.Visible;
            });

            if (InputPorts[0].Data.GetType() == typeof(ModelInfo))
            {
                modelInfo = InputPorts[0].Data as ModelInfo;

                if (modelInfo != null && modelInfo.ModelType == ModelType.BimPlusModel)
                {
                    // Get the corresponding model
                    var model = _controller.BimPlusModels[Guid.Parse(modelInfo.ModelId)];
                    if (model == null) return;

                    var list = model.Objects.Where(item => modelInfo.ElementIds.Contains(item.Id)).ToList();
                    doWorkEventArgs.Result = VisualizeBimPlusDataAsGenericElements(list, sender);
                }
            }
            else if (InputPorts[0].Data.GetType() == typeof(List<DtObject>))
            {
                doWorkEventArgs.Result = VisualizeBimPlusDataAsGenericElements(InputPorts[0].Data as List<DtObject>, sender);
            }
            else if (InputPorts[0].Data.GetType() == typeof(List<DbGeometry>))
            {
                // .Result = VisualizeBimPlusData(InputPorts[0].Data as List<GenericElement>, sender);
            }
            else if (InputPorts[0].Data.GetType() == typeof(Relation))
            {
                var input = InputPorts[0].Data as Relation;
                if (input == null)
                    return;
                // Get Model
                var model = _controller.BimPlusModels[input.ModelId];

                // To be visualized elements
                var visElements1 = new List<DtObject>();
                var visElements2 = new List<DtObject>();
                // var visElements3 = new List<DtObject>();

                // ID Lists from the relation
                var listGuids1 = new List<Guid>();
                var listGuids2 = new List<Guid>();

                var collection = input.Collection as ObservableCollection<Tuple<Guid, Guid>>;
                if (collection != null)
                    foreach (var item in collection)
                    {
                        listGuids1.Add(item.Item1);
                        listGuids2.Add(item.Item2);
                    }

                // Setup the visualizing element list
                foreach (var elem in model.Objects)
                {
                    if (listGuids1.Contains(elem.Id))
                    {
                        if(!visElements1.Contains(elem))
                            visElements1.Add(elem);
                    }
                    if (listGuids2.Contains(elem.Id))
                    {
                        if (!visElements2.Contains(elem))
                            visElements2.Add(elem);
                    }
                    // else
                    // {
                    //     visElements3.Add(elem);
                    // }
                }

                var list = new List<MeshIdandGeometry>();
                list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements1, sender, Colors.Red, 0.6));
                list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements2, sender, Colors.Blue, 0.6));
                // list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements3, sender, default(Color), 0.95));

                doWorkEventArgs.Result = list;
                
            }
            else if (InputPorts[0].Data.GetType() == typeof(Relation3))
            {
                var input = InputPorts[0].Data as Relation3;
                if (input == null)
                    return;
                // Get Model
                var model = _controller.BimPlusModels[input.ModelId];

                // To be visualized elements
                var visElements1 = new List<DtObject>();
                var visElements2 = new List<DtObject>();
                var visElements3 = new List<DtObject>();

                // ID Lists from the relation
                var listGuids1 = new List<Guid>();
                var listGuids2 = new List<Guid>();
                var listGuids3 = new List<Guid>();
                var collection = input.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;
                if (collection != null)
                    foreach (var item in collection)
                    {
                        listGuids1.Add(item.Item1);
                        listGuids2.Add(item.Item2);
                        listGuids3.Add(item.Item3);
                    }

                // Setup the visualizing element list
                foreach (var elem in model.Objects)
                {
                    if (listGuids1.Contains(elem.Id))
                    {
                        if (!visElements1.Contains(elem))
                            visElements1.Add(elem);
                    }
                    if (listGuids2.Contains(elem.Id))
                    {
                        if (!visElements2.Contains(elem))
                            visElements2.Add(elem);
                    }
                    if (listGuids3.Contains(elem.Id))
                    {
                        if (!visElements3.Contains(elem))
                            visElements3.Add(elem);
                    }
                }

                var list = new List<MeshIdandGeometry>();
                list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements1, sender, Colors.Red, 0.6));
                list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements2, sender, Colors.Blue, 0.6));
                list.AddRange(VisualizeBimPlusDataAsGenericElements(visElements3, sender, Colors.BlueViolet, 0.6));

                doWorkEventArgs.Result = list;
            }
            else
            {
                var s = InputPorts[0].Data as string;
                if (s != null)
                    doWorkEventArgs.Result = ReadFileData(s);
            }
        }

        public List<MeshIdandGeometry> VisualizeBimPlusDataAsGenericElements(List<DtObject> baseElements, object sender, Color Color = default(Color), double transparency = 0)
        {
            if (InputPorts[0].Data == null)
                return null;

            var max = baseElements.Count;
            var m_i = 1;

            // Init some lists and containers
            var container = new ContainerUIElement3D();
            var geometry = new List<MeshIdandGeometry>();

            // Init the MeshBuilde
            var meshBuilder = new MeshBuilder(false, false);

            // Loop the items of each list
            foreach (var item in baseElements)
            {
                var color = item.AttributeGroups["geometry"].GetProperty("color") is System.Drawing.Color ? (System.Drawing.Color) item.AttributeGroups["geometry"].GetProperty("color") : new System.Drawing.Color();

                Color col;
                if (Color != default(Color))
                {
                    col = Color;
                }
                else
                {
                    // Check Color
                    col = new Color{A = color.A,G = color.G,R = color.R,B = color.B};
                }

                var brush = new SolidColorBrush { Color = col, Opacity = 1 - transparency };

                var myGeometryModel = new GeometryModel3D
                {
                    Material = new DiffuseMaterial(brush),
                    BackMaterial = new DiffuseMaterial(brush),
                    Geometry = MeshObject(item, meshBuilder),
                    Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90))
                };

                myGeometryModel.Freeze();

                // Save the parsed data in the objects
                var meshAndId = new MeshIdandGeometry
                {
                    Id = item.Id,
                    Model3D = myGeometryModel,
                    Material = myGeometryModel.Material
                };

                geometry.Add(meshAndId);
                // Save the parsed information in the elements attribute group
                item.AttributeGroups["geometry"]["parsedGeometry"] = meshAndId;

                // Refresh the builder so that we do not duplicate the meshes 
                meshBuilder = new MeshBuilder(false, false);

                m_i++;
                var progressPercentage = Convert.ToInt32(((double)m_i / max) * 100);
                var backgroundWorker = sender as BackgroundWorker;
                backgroundWorker?.ReportProgress(progressPercentage, item.Id);
            }

            container.Children.Clear();
            return geometry;
        }

        public MeshGeometry3D MeshObject(DtObject item, MeshBuilder meshBuilder)
        {
            var points = item.AttributeGroups["geometry"].GetProperty("threejspoints") as List<Point3D>;
            var indices = item.AttributeGroups["geometry"].GetProperty("geometryindices") as List<uint>;

            if (indices == null)
                return null;

            for (var i = 0; i < indices.Count; i++)
            {
                switch (indices[i])
                {
                    case 0:
                        meshBuilder.AddTriangle(points[(int)indices[i + 1]], points[(int)indices[i + 2]],
                            points[(int)indices[i + 3]]);

                        i = i + 3;
                        break;
                    case 1:
                        meshBuilder.AddQuad(points[(int)indices[i + 1]], points[(int)indices[i + 2]],
                            points[(int)indices[i + 3]],
                            points[(int)indices[i + 4]]);
                        i = i + 4;
                        break;
                }
            }
            return meshBuilder.ToMesh(true);
        }

        /// <summary>
        ///     OnElementMouseDown: Click Event for each geometric element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="watch3DNode"></param>
        private void OnElementMouseDown(object sender, MouseButtonEventArgs e, Watch3DNode watch3DNode)
        {
            // 1-CLick event
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Get sender
            var element = sender as ModelUIElement3D;
            var geometryModel3D = element?.Model as GeometryModel3D;
            if (geometryModel3D == null)
                return;

            // Check Type
            // If it is already selected ... Deselect
            if (SelectedModels.Contains(geometryModel3D))
            {
                // var geo = geometryModel3D.Clone();
                // geo.Material = geometryModel3D.BackMaterial;
                geometryModel3D.Material = geometryModel3D.BackMaterial;
                SelectedModels.Remove(geometryModel3D);
            }
            // If not ... Select!
            else
            {
                SelectedModels.Add(geometryModel3D);
                // var geo = geometryModel3D.Clone();
                // geo.Material = _selectionMaterial;
                geometryModel3D.Material = _selectionMaterial;
            }

            // Get the id of the selected model
            var reslist = new List<Guid>();
            
            if (SelectedModels != null && SelectedModels.Count > 0)
            {
                foreach (var item in _fallBackGeometry)
                {
                    foreach (var item_2 in SelectedModels)
                    {
                        if (Equals(item_2.Geometry, item.Model3D.Geometry))
                            reslist.Add(item.Id);
                    }
                }
            }

            string modelId = null;
            string projectId = null;

            if (InputPorts[0].Data.GetType() == typeof(ModelInfo))
            {
                var input = InputPorts[0].Data as ModelInfo;
                if (input != null)
                {
                    modelId = input.ModelId;
                    projectId = input.ProjectId;
                }
            }
            else if (InputPorts[0].Data.GetType() == typeof(BaseRelation))
            {
                var input = InputPorts[0].Data as BaseRelation;
                if (input != null)
                {
                    modelId = input.ModelId.ToString();
                    projectId = input.ProjectId.ToString();
                }
            }
            else
            {
                return;
            }

            var res = new ModelInfo(projectId, modelId, reslist, ModelType.BimPlusModel);

            OutputPorts[0].Data = res;
            // Set selected models to Output ...  
            
            e.Handled = true;
        }

    }
}
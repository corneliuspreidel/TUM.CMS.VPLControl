using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc2x3.GeometricConstraintResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.UtilityResource;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Controls
{
    /// <summary>
    /// Interaction logic for IFCViewerControl.xaml
    /// </summary>
    public partial class IFCViewerControl : UserControl
    {
        private ModelController ModelController;
        private MeshBuilder meshBuilder;

        // BackgroundWorker 
        private BackgroundWorker _worker;

        private IfcModel model;
        private IfcStore xModel;
        private Xbim3DModelContext context;

        // Selection Stuff
        private readonly Material _selectionMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Crimson));
        private Dictionary<string, DiffuseMaterial> tempMaterialLibrary;
        public Dictionary<string, GeometryModel3D> SelectedModels;
        private Dictionary<string, ModelUIElement3D> VisualizedModels;

        private List<IfcGloballyUniqueId> elementIdsList;

        public EventHandler onElementMouseDown;


        public IFCViewerControl()
        {
            InitializeComponent();
            InitViewPort();

            ModelController = ModelController.Instance;
        }

        public void InitViewPort()
        {
            Viewport3D.Children.Clear();
            Viewport3D.ShowTriangleCountInfo = true;
            Viewport3D.Children.Add(new SunLight());
            Viewport3D.ZoomExtentsWhenLoaded = true;
        }

        public List<ModelUIElement3D> CreateModelUiElementsDs(IfcModel model, List<IfcGloballyUniqueId> elementIdsList, bool visualizeUnselectedElementsTransparent = true, DiffuseMaterial overrideMaterial = null)
        {
            tempMaterialLibrary = new Dictionary<string, DiffuseMaterial>();
            // Refresh the selected Models
            SelectedModels = new Dictionary<string, GeometryModel3D>();
            VisualizedModels = new Dictionary<string, ModelUIElement3D>();

            xModel = model.GetModel();
            context = model.GetModelContext();

            // Loop through Entities and visualze them in the viewport
            var res = new HashSet<IfcGloballyUniqueId>(elementIdsList);

            var elementList = new List<ModelUIElement3D>();

            // GET GEOREFERENCING
            var wcsTransformation = new XbimMatrix3D();
            var myIfcSite = xModel.Instances.OfType<Xbim.Ifc2x3.ProductExtension.IfcSite>();
            var ifcSites = myIfcSite as IList<Xbim.Ifc2x3.ProductExtension.IfcSite> ?? myIfcSite.ToList();
            if (ifcSites.Count == 1)
            {
                Xbim.Ifc2x3.ProductExtension.IfcSite mySite = ifcSites.First();
                IfcLocalPlacement relplacement = mySite.ObjectPlacement.ReferencedByPlacements.First();
                wcsTransformation = relplacement.ToMatrix3D();
            }

            foreach (var item in xModel.Instances.OfType<IIfcProduct>())
            {
                if (visualizeUnselectedElementsTransparent == false)
                {
                    if (!res.Contains(item.GlobalId))
                        continue;
                }
                // Get the Material
                var mat = new DiffuseMaterial();
                if (overrideMaterial == null)
                {
                    mat = res.Contains(item.GlobalId)
                        ? GeometryHandler.GetStyleFromXbimModel(item, context)
                        : GeometryHandler.GetStyleFromXbimModel(item, context, 0.03);
                }
                else
                    mat = overrideMaterial;

                var m = GeometryHandler.WriteTriangles(item, context, wcsTransformation);
                tempMaterialLibrary.Add(item.GlobalId, mat);
                var element = CreateModelUIElement3D(m, mat);
                element.MouseDown += ElementOnMouseDown;
                elementList.Add(element);
                VisualizedModels.Add(item.GlobalId, element);
            }
            return elementList;
        }

        public void Visualize(List<ModelUIElement3D> elementList, bool initViewport = true)
        {
            if(initViewport)
                InitViewPort();

            // Add elements to the View Port
            foreach (var element in elementList)
            {
                if (element != null)
                    Viewport3D.Children.Add(element);
            }
        }

        public void VisualizeWithWorker(IfcModel model, List<IfcGloballyUniqueId> elementIdsList)
        {
            this.model = model;
            this.elementIdsList = elementIdsList;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            // _worker.RunWorkerAsync(file);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            _worker.RunWorkerAsync(10000);
        }


        public void worker_DoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            doWorkEventArgs.Result = CreateModelUiElementsDs(model, elementIdsList);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var elementList = e.Result as List<ModelUIElement3D>;
            Visualize(elementList);
        }

        private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 1-CLick event
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            // Get sender
            var element = sender as ModelUIElement3D;

            // Get the ID 
            if (!VisualizedModels.ContainsValue(element))
                return;

            var myKey = VisualizedModels.FirstOrDefault(x => Equals(x.Value, element)).Key;


            var geometryModel3D = element?.Model as GeometryModel3D;
            if (geometryModel3D == null)
                return;

            if (SelectedModels.ContainsKey(myKey))
            { 
                SelectedModels[myKey].Material = SelectedModels[myKey].BackMaterial;
                SelectedModels.Remove(myKey);
            }
            else
            {
                var geo = geometryModel3D.Clone();
                geo.Material = _selectionMaterial;
                geometryModel3D.Material = _selectionMaterial;
                SelectedModels.Add(myKey, geometryModel3D);
            }

            onElementMouseDown.Invoke(SelectedModels, new EventArgs());
            e.Handled = true;
        }

    
        /// <summary>
        ///     VisualizeMesh in the Viewport
        /// </summary>
        /// <param name="meshBuilder"></param>
        /// <param name="mesh"></param>
        /// <param name="mat"></param>
        /// <param name="itemModel"></param>
        /// <param name="indexOfModel"></param>
        public ModelUIElement3D CreateModelUIElement3D(MeshGeometry3D mesh, DiffuseMaterial mat)
        {
            var myGeometryModel = new GeometryModel3D
            {
                Material = mat,
                BackMaterial = mat,
                Geometry = mesh //meshBuilder.ToMesh(true)
            };
        
            var element = new ModelUIElement3D { Model = myGeometryModel };
        
            return element;
        }
    }
}

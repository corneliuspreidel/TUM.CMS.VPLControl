using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace TUM.CMS.VplControl.Watch3Dx.Controls
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Watch3DxControl
    {
        private MeshBuilder mb;

        public Watch3DxControl()
        {
            InitializeComponent();
            // DataContext = this;

            // titles
            view1.Title = "Watch3DxNode";
            view1.SubTitle = "SharpDX";

            // camera setup
            view1.Camera = new PerspectiveCamera
            {
                Position = new Point3D(3, 3, 5),
                LookDirection = new Vector3D(-3, -3, -5),
                UpDirection = new Vector3D(0, 1, 0),
                FarPlaneDistance = 5000000
            };

            // default render technique
            view1.RenderTechniquesManager = new DefaultRenderTechniquesManager();
            view1.RenderTechnique = view1.RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Phong];
            view1.EffectsManager = new DefaultEffectsManager(view1.RenderTechniquesManager);

            // Setup lights
            var directionalLight3D = new DirectionalLight3D
            {
                Color = Color.White,
                Direction = new Vector3(-2, -5, -2)
            };

            var ambientLight3D = new AmbientLight3D {Color = Color.White};

            view1.Items.Add(ambientLight3D);
            view1.Items.Add(directionalLight3D);

            // Geometry
            var mb = new MeshBuilder();
            mb.AddSphere(new Vector3(0, 0, 0), 0.5);
            mb.AddBox(new Vector3(0, 0, 0), 1, 0.5, 2, BoxFaces.All);
            var meshGeometry = mb.ToMeshGeometry3D();

            var meshGeomModel = new MeshGeometryModel3D
            {
                Geometry = meshGeometry,
                Material = PhongMaterials.Red
            };
            // Add the Model to the viewport
            view1.Items.Add(meshGeomModel);
        }

        public void AddMeshGeometry(MeshGeometryModel3D meshGeometry)
        {
            view1.Items.Add(meshGeometry);
        }

        // public void LoadObjModel(string filename, MeshFaces faces)
        // {
        //     RemoveGeometryModels();
        // 
        //     // Change to Tesselation
        //     view1.RenderTechniquesManager = new TessellationTechniquesManager();
        //     view1.RenderTechnique = view1.RenderTechniquesManager.RenderTechniques[TessellationRenderTechniqueNames.PNQuads];
        //     view1.EffectsManager = new TessellationEffectsManager(view1.RenderTechniquesManager);
        // 
        // 
        //     if (!filename.EndsWith(".obj"))
        //         return;
        //     
        //     // load model
        //     var reader = new ObjReader();
        //     var objModel = reader.Read(filename, new ModelInfo() { Faces = faces });
        //     
        //     foreach (var item in objModel)
        //     {
        //         var geometry = item.Geometry as HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
        //         if (geometry != null)
        //             geometry.Colors = new Color4Collection(geometry.Positions.Select(x => new Color4(1, 0, 0, 1)));
        //     
        //         var patchGeomModel = new PatchGeometryModel3D()
        //         {
        //             Geometry = geometry,
        //             Material = item.Material,
        //             Transform = new TranslateTransform3D(0, -0, 0)
        //         };
        //     
        //         view1.Items.Add(patchGeomModel);
        //     }
        // }

        public void RemoveGeometryModels()
        {
            view1.Items.Clear();
            SetupLights();
            SetupGrid();

            // foreach (var item in view1.Items)
            // {
            //     if (item.GetType() == typeof(GeometryModel3D) || item.GetType().IsSubclassOf(typeof(GeometryModel3D)))
            //     {
            //         view1.Items.Remove(item);
            //     }
            // }
        }

        public void SetupLights()
        {
            // Setup lights
            var directionalLight3D = new DirectionalLight3D
            {
                Color = Color.White,
                Direction = new Vector3(-2, -5, -2)
            };
            var ambientLight3D = new AmbientLight3D
            {
                Color = Color.White
            };

            view1.Items.Add(ambientLight3D);
            view1.Items.Add(directionalLight3D);
        }

        public void SetupGrid()
        {
            // floor plane grid
            var lineGeom = new LineGeometryModel3D
            {
                Geometry = LineBuilder.GenerateGrid(),
                Color = Color.Black,
                Transform = new TranslateTransform3D(-5, -4, -5)
        };

            view1.Items.Add(lineGeom);

        }
    }
}
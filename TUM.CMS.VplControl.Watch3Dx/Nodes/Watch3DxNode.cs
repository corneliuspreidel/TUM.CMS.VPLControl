using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Watch3Dx.Controls;
using TUM.CMS.VplControl.Watch3Dx.Utilities;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;

namespace TUM.CMS.VplControl.Watch3Dx.Nodes
{
    public class Watch3DxNode : Node
    {
        public Watch3DxControl _control;
        private MainViewModel mainViewModel;

        private Camera camera;

        private RenderTechnique renderTechnique;


        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="hostCanvas"></param>
        public Watch3DxNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Input 
            AddInputPortToNode("Object", typeof (object));

            // mainViewModel = new MainViewModel();
            // mainViewModel = new MainViewModel();

            IsResizeable = true;
            // _control = new Watch3DxControl {DataContext = mainViewModel};
            _control = new Watch3DxControl();
            AddControlToNode(_control);
        }

        /// <summary>
        ///     Calculation Function
        /// </summary>
        public override void Calculate()
        {
            // _control.view1.Reset();
            var s = InputPorts[0].Data;

            if (s.GetType() == typeof(string))
            {
                // _control.LoadObjModel(s as string, MeshFaces.Default);
            }
        }

        public void VisualizeMesh(MeshGeometryModel3D model)
        {
            _control.AddMeshGeometry(model);
        }


        public override Node Clone()
        {
            return new Watch3DxNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
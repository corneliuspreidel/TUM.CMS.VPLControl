﻿using System.Linq;
using System.Reflection;
using System.Windows.Input;
using TUM.CMS.VplControl.Utilities;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Watch3D.Nodes;
using TUM.CMS.VPL.Scripting.Nodes;

namespace TUM.CMS.VplControl.Test
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            KeyDown += VplControl.VplControl_KeyDown;
            KeyUp += VplControl.VplControl_KeyUp;

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Test.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Watch3D.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.Add(typeof(Watch3DNode));
            VplControl.ExternalNodeTypes.Add(typeof (ScriptingNode));
            VplControl.NodeTypeMode = NodeTypeModes.All;
            // VplPropertyGrid.SelectedObject = VplControl.Theme;
            // VplPropertyGrid.SelectedObject = VplControl;
        }

        /// <summary>
        /// Override Function for the ZoomBox MouseWheel Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoombox_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                // Zoombox.Zoom(0.1);
            }
            else
            {
                // Zoombox.Zoom(-0.1);
            }
        }

        /// <summary>
        /// Overriding function for the zoombox panning event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoombox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton != MouseButtonState.Pressed) return;

            VplControl.MouseMode = MouseModes.Panning;

            foreach (var node in VplControl.NodeCollection)
            {
                node.OldMousePosition = e.GetPosition(this);
                if (VplControl.MouseMode != MouseModes.Panning)
                    MouseMove += node.HostCanvas_MouseMove;
            }
            //VplPropertyGrid.SelectedObject = VplControl;
        }
    }
}
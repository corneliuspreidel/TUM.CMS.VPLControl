using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Watch3D.Controls;

namespace TUM.CMS.ExtendedVplControl.Ports.Output
{
    public class Watch3DPort: ExtendedPort
    {
        public Watch3DControl watch3DControl;
        protected HelixViewport3D HelixViewport3D { get; set; }

        public Watch3DPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            watch3DControl = new Watch3DControl();
            DataChanged += OnDataChanged;

            // Init Viewport
            HelixViewport3D = watch3DControl.ViewPort3D;
            HelixViewport3D.Title = "Watch3D";

            AddPopupContent(watch3DControl);

            MouseWheel += OnMouseWheel;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; 
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            if (Data == null)
                return; 

            // Clear the viewport
            HelixViewport3D.Children.Clear();

            // Check the input 
            // -> string = filepath
            // -> list of strings = multiple filepaths
            // -> ContainerUiElements = Geometric Elements
            if (Data is string)
            {
                var model = ReadFileData((string)Data);
                if (model == null) return;

                HelixViewport3D.Children.Add(model);
            }
            else if (Data.GetType() == typeof(List<object>))
            {
                foreach (var str in Data as List<object>)
                {
                    var model = ReadFileData((string)str);

                    if (model != null)
                        HelixViewport3D.Children.Add(model);
                }
            }
            else if (Data is ContainerUIElement3D)
            {
                HelixViewport3D.Children.Add(Data as ContainerUIElement3D);
            }

            // Zoom 
            HelixViewport3D.CameraController.ZoomExtents();
            // Set Standard Light
            HelixViewport3D.Children.Add(new DefaultLights());
        }

        /// <summary>
        ///     Read FileData
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ContainerUIElement3D ReadFileData(string path)
        {
            var extension = Path.GetExtension(path);
            // var visModel = new ModelVisual3D();
            var container = new ContainerUIElement3D();

            switch (extension)
            {
                case ".obj":
                    var currentHelixObjReader = new ObjReader();
                    try
                    {
                        var myModel = currentHelixObjReader.Read(path);

                        foreach (var model in myModel.Children)
                        {
                            if (model is GeometryModel3D)
                            {
                                var geom = model as GeometryModel3D;

                                var element = new ModelUIElement3D { Model = geom };
                                // element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this);
                                container.Children.Add(element);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                    break;
                case ".stl":
                    var currentHelixStlReader = new StLReader();
                    try
                    {
                        var myModel = currentHelixStlReader.Read(path);

                        foreach (var model in myModel.Children)
                        {
                            if (model is GeometryModel3D)
                            {
                                var geom = model as GeometryModel3D;

                                var element = new ModelUIElement3D { Model = geom };
                                // element.MouseDown += (sender1, e1) => OnElementMouseDown(sender1, e1, this);
                                container.Children.Add(element);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }
                    break;
            }

            return container;
        }
    }
}

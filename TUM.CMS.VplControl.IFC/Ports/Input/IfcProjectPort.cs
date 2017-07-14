using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Controls;
using TUM.CMS.VplControl.IFC.Utilities;
using TUM.CMS.VplControl.Utilities;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace TUM.CMS.VplControl.IFC.Ports.Input
{
    public class IfcProjectPort : ExtendedPort
    {
        private ModelController modelController;
        private IfcProjectPortControl control;

        private BackgroundWorker _worker;

        private ModelInfo modelInfo;

        public IfcProjectPort(string name, PortTypes portType, Type type, Core.VplControl hostCanvas)
            : base(name, portType, type, hostCanvas)
        {
            modelController = ModelController.Instance;

            control = new IfcProjectPortControl();
            control.OpenFileButton.Click += ButtonOnClick;
            
            AddPopupContent(control);

            // MouseDown += OnMouseDown;
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "IFC (.ifc)|*.ifc"
            };

            string file = ""; 

            if (openFileDialog.ShowDialog() == true)
            {
                file = openFileDialog.FileName;
                if (file != "" && File.Exists(file))
                {
                    _worker = new BackgroundWorker();
                    _worker.DoWork += new DoWorkEventHandler(worker_DoWork);
                    _worker.RunWorkerAsync(file);
                    _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                }
                else
                {
                   // Error Message
                }
            }

        }

        /// <summary>
        /// Background Worker
        /// 
        /// Create xBIM File and Add it to the new DataController.
        /// The xBIM File is therefor stored in a Database
        /// 
        /// The ModelId (FilePath) and the ElementList are stored in the ModelInfo Class
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Value File is the FilePath</param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument.ToString();

            var model = new IfcModel(ModelTypes.IFC, filePath);
            
            using (model.xModel = IfcStore.Open(filePath))
            {
                // Create context
                model.xModelContext = new Xbim3DModelContext(model.xModel);
                model.xModelContext.CreateContext();

                modelInfo = new ModelInfo();
                foreach (var item in model.xModel.Instances.OfType<IIfcProduct>())
                {
                    modelInfo.elementIds.Add(item.GlobalId.ToString());
                }
                e.Result = modelInfo;
            }

            modelInfo.modelId = model.id.ToString();
            modelController.AddModel(model);
            model.xModel.Close();
            
            e.Result = model;
        }

        /// <summary>
        /// Its important to split the Creation of the xBIM File and the OutputPort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Result is the modelInfo Class</param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        { 
           Data = modelInfo;
           control.FileNameTextBox.Text = modelController.GetModel(modelInfo.modelId).name;
        }

    }
}

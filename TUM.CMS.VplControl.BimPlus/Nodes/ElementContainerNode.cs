using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using BimPlus.Sdk.Data.DbCore;
using BimPlus.Sdk.Data.Geometry;
using BimPlus.Sdk.Data.TenantDto;
using Nemetschek.NUtilLibrary;
using Newtonsoft.Json.Linq;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementContainerNode : UtilityNode
    {
        private readonly DataController _controller;
        private List<DtObject> _elements;
        private DtoModel _model;

        private Guid projectId;

        private BackgroundWorker _worker;
        private ElementContainerNodeControl _control;

        public ElementContainerNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            _control = new ElementContainerNodeControl();
            _control.cancelBut.Click += CancelButOnClick;

            AddControlToNode(_control);

            //Input
            AddInputPortToNode("Input", typeof (object));
            // Output
            AddOutputPortToNode("Elements", typeof (ModelInfo));

            DataContext = this;
        }

        private void CancelButOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_worker.IsBusy)
                _worker.CancelAsync();

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                _control.LoadingGrid.Visibility = Visibility.Collapsed;
                _control.statusLabel.Content = "Status: Loading cancelled ...";
            });
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null) return;

            _worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _worker.DoWork += WorkerOnDoWork;
            _worker.ProgressChanged += WorkerOnProgressChanged;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
            _worker.RunWorkerAsync(10000);
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            _control.progressBar.Value = progressChangedEventArgs.ProgressPercentage;

            Dispatcher.BeginInvoke((Action)delegate()
            {
                _control.statusLabel.Content = "Status: Loading ..." + progressChangedEventArgs.ProgressPercentage + " %";
            });

        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            // Shoe ProgressBar
            Dispatcher.BeginInvoke((Action)delegate () {
                _control.LoadingGrid.Visibility = Visibility.Visible;
                _control.statusLabel.Content = "Status: Loading ...";
            });

            if (InputPorts[0] == null || ControlElements[0] == null || InputPorts[0].Data == null) return;

            if (InputPorts[0].Data.GetType() == typeof(DtoShortProject))
            {
                
                var project = InputPorts[0].Data as DtoShortProject;

                if (project != null)
                {
                    projectId = project.Id;
                    var topo = _controller.IntBase.ApiCore.Projects.GetTopology(project.Id);
                    if (topo == null)
                        return;
                    var model = _controller.IntBase.ApiCore.DtObjects.GetObjectGeometryAsThreeJs(topo.Id);

                    if (model.Objects == null)
                        return;

                    foreach (var elem in model.Objects)
                    {
                        var geom = new DbGeometry();
                        var jObject = elem.AttributeGroups["geometry"]["threejs"] as JObject;
                        if (jObject != null)
                        {
                            geom.Vertices = jObject.SelectToken("vertices").ToList().Select(value => value.Value<double>()).ToList();
                            geom.Faces = jObject.SelectToken("faces").ToList().Select(value => value.Value<uint>()).ToList();

                            var colorid = (int)jObject.SelectToken("metadata")["colorid"];
                            var colorInteger = model.Colors[colorid];
                            var color = Color.FromArgb((int)colorInteger);

                            GeometryWriter.EvaluateAndSaveGeometry(geom, elem, color);
                        }
                    }
                    
                    // model.Colors
                    // Color c = Color.FromArgb(someInt);

                    // var model = _controller.IntBase.APICore.DtObjects.GetObjectGeometry(topo.Id);
                    _elements = (List<DtObject>) model.Objects;
                }
                // _elements = _controller.IntBase.APICore.GetElementsFromTopologyId(project.Id);
                // OutputPorts[0].Data = _elements;
            }

            if (InputPorts[0].Data.GetType() == typeof(DtoDivision))
            {
                var dtoDivision = InputPorts[0].Data as DtoDivision;
                if (dtoDivision?.TopologyDivisionId != null)
                {
                    var model = _controller.IntBase.ApiCore.DtObjects.GetObjectGeometryAsThreeJs((Guid)dtoDivision.TopologyDivisionId);
                    projectId = dtoDivision.ProjectId;

                    foreach (var elem in model.Objects)
                    {
                        var geom = new DbGeometry();
                        var jObject = elem.AttributeGroups["geometry"]["threejs"] as JObject;
                        if (jObject != null)
                        {
                            geom.Vertices = jObject.SelectToken("vertices").ToList().Select(value => value.Value<double>()).ToList();
                            geom.Faces = jObject.SelectToken("faces").ToList().Select(value => value.Value<uint>()).ToList();

                            var colorid = (int)jObject.SelectToken("metadata")["colorid"];
                            var colorInteger = model.Colors[colorid];
                            var color = Color.FromArgb((int)colorInteger);

                            GeometryWriter.EvaluateAndSaveGeometry(geom, elem, color);
                        }
                    }
                    _elements = (List<DtObject>)model.Objects;
                }
            }

            doWorkEventArgs.Result = _elements;
        }

        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (_elements != null)
            {
                Dispatcher.BeginInvoke((Action)delegate () {
                    _control.LoadingGrid.Visibility = Visibility.Collapsed;
                    _control.statusLabel.Content = "Status: " +_elements.Count+ " Elements loaded!";
                });
            }
            // Store the data in the dataController
            if (_elements != null)
            {
                var guid = Guid.NewGuid();
                _model = new DtoModel {Objects = _elements.ToList()};
                _controller.BimPlusModels.Add(guid, _model);
                var output = new ModelInfo(projectId.ToString(), guid.ToString(), _elements.Select(item => item.Id).ToList(), ModelType.BimPlusModel);
                OutputPorts[0].Data = output;
            }
        }

        public override Node Clone()
        {
            return new ElementContainerNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        public void LoadGeometricRepresentation(List<DtObject> elements)
        {
            try
            {
                foreach (var elem in elements)
                {
                    if(elem.Id != Guid.Empty)
                        _controller.IntBase.ApiCore.DtObjects.GetObjectGeometryAsThreeJs(elem.Id);

                    if (elem.Id == Guid.Empty)
                        elem.Id = Guid.NewGuid();

                    var data = elem.AttributeGroups["geometry"]["picture"].ToString();

                    if (data != null)
                    {
                        var bytePolyeder = Convert.FromBase64String(data);
                        var NULLcompress = Convert.ToInt32(elem.AttributeGroups["geometry"]["compress"]);
                        var compress = (byte) (NULLcompress == null ? 1 : NULLcompress);

                        CBaseElementPolyeder poly = null;
                        try
                        {
                            if (Convert.ToInt32(elem.AttributeGroups["geometry"]["type"]) == 7)
                                poly = CsgGeometry.Create(bytePolyeder, compress);
                            else
                            {
                                poly = DeserializeVarbinaryMax.DeserializePolyeder(bytePolyeder, compress);

                                if (poly.face.Count == 0 && poly.edge.Count > 0)
                                    poly = Tubes.CreateTube(poly);
                            }
                        }
                        catch (Exception)
                        {
                        }

                        var stringMatrix = elem.AttributeGroups["geometry"]["matrix"].ToString();
                        if (!string.IsNullOrEmpty(stringMatrix) && poly != null)
                        {
                            /*
                            var mat = new CMatrix(Convert.FromBase64String(stringMatrix));
                            poly.MultiplyWithMatrix(mat.Values);
                            */
                        }

                        var col = elem.AttributeGroups["geometry"]["color"];
                        // try
                        // {
                        //     elem.Material = col != null
                        //         ? Color.FromArgb((int) Convert.ToUInt32(col))
                        //         : Color.Transparent;
                        // }
                        // catch (Exception)
                        // {
                        //     baseElem.Material = col != null ? Color.FromArgb(Convert.ToInt32(col)) : Color.Transparent;
                        // }
                        // 
                        // if (col == null || baseElem.Material == Color.FromArgb(255, 128, 128, 128))
                        // {
                        //     if (poly != null)
                        //     {
                        //         baseElem.Material = poly.IsSpecialColorDefined()
                        //             ? Color.FromArgb((int) poly.GetSpecialColor())
                        //             : Color.FromArgb(255, 128, 128, 128);
                        //     }
                        //     else
                        //         baseElem.Material = Color.FromArgb(255, 128, 128, 128);
                        // }
                        // 
                        // if (poly != null)
                        // {
                        //     poly.ID = baseElem.Id;
                        //     baseElem.Polyeder = poly;
                        //     poly.Radius = null;
                        //     poly.Bendingrolle = null;
                        // }
                        // else
                        // {
                        //     Debug.Print(
                        //         $"Invalid geometry for {((GenericElement) baseElem).TypeName} with ID {baseElem.Id}!");
                        // }
                    }

                    // elem.LoadState |= ElementLoadState.GeometryLoaded;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
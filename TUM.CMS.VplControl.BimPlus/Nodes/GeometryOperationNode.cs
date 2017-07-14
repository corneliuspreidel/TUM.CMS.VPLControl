using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BimPlus.Sdk.Data.DbCore;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using QL4BIMspatial;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Relations.Nodes;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class GeometryOperationNode : RelationalObjectNode
    {
        // DataController
        private readonly DataController _controller;
        private readonly ComboBox _typeComboBox;
        private readonly Button _button;

        // QL4BIM Inits
        private static UnityContainer container = new UnityContainer();
        private static MainInterface ql4Spatial = new MainInterface(container);

        public GeometryOperationNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Init the QL4BIM framework
            // Commented by CP on 24.04
            
            var settings = ql4Spatial.GetSettings();
            
            // Input
            AddInputPortToNode("InputElements_1", typeof(List<DtObject>));
            // Input
            AddInputPortToNode("InputElements_2", typeof(List<DtObject>));
            // Output
            AddOutputPortToNode("Relation", typeof(Relation));

            // Control
            _typeComboBox = new ComboBox();
            _typeComboBox.Items.Add("Overlap");
            _typeComboBox.Items.Add("Touch");
            _typeComboBox.Items.Add("Contain");
            _typeComboBox.Items.Add("AboveOf");
            _typeComboBox.Items.Add("BelowOf");
            _typeComboBox.Items.Add("WestOf");
            _typeComboBox.Items.Add("EastOf");
            _typeComboBox.Items.Add("NorthOf");
            _typeComboBox.Items.Add("SouthOf");
            _typeComboBox.SelectedItem = _typeComboBox.Items.GetItemAt(0);
            AddControlToNode(_typeComboBox);

            _button = new Button {Content = "Execute"};
            _button.Click += ButtonOnClick;
            AddControlToNode(_button);
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (InputPorts[0].Data == null || InputPorts[1].Data == null)
                return;

            var modelInfo1 = InputPorts[0].Data as ModelInfo;
            var modelInfo2 = InputPorts[1].Data as ModelInfo;

            var result = new Relation(Guid.Parse(modelInfo1.ModelId), Guid.Parse(modelInfo1.ProjectId));
            var resCollection = result.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            if (modelInfo1 != null && modelInfo2 != null)
            {
                var list_1 = CreateTriangleMeshes(modelInfo1);
                var list_2 = CreateTriangleMeshes(modelInfo2);

                // Init the Settings for the operators
                var settings = container.Resolve<ISettings>();

                settings.Direction.RaysPerSquareMeter = 100;
                settings.Direction.PositiveOffset = 100;
                // The mapping of the operators is because of the transformation of the model

                if (_typeComboBox != null && _typeComboBox.SelectedItem == "Overlap")
                {
                    // Init the Operator 
                    var op = container.Resolve<IOverlapOperator>();
                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            var abs = op.Overlap(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "Touch")
                {
                    var op = container.Resolve<ITouchOperator>();
                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            var abs = op.Touch(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "Contain")
                {
                    // Init the Operator 
                    var op = container.Resolve<IContainOperator>();
                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            var abs = op.Contain(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "AboveOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    // Init the Operator 
                    var op2 = container.Resolve<IOverlapOperator>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.WestOfRelaxed(item1, item2);
                            if (abs)
                            {
                                if(!op2.Overlap(item1, item2))
                                    resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                            }
                               
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "BelowOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    // Init the Operator 
                    var op2 = container.Resolve<IOverlapOperator>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.EastOfRelaxed(item1, item2);
                            if (abs)
                            {
                                if (!op2.Overlap(item1, item2))
                                    resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                            }
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "SouthOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.SouthOfRelaxed(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "NorthOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.NorthOfRelaxed(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "EastOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.AboveOfRelaxed(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
                else if (_typeComboBox != null && _typeComboBox.SelectedItem == "WestOf")
                {
                    // Init the Operator 
                    var op = container.Resolve<IDirectionalOperators>();

                    foreach (var item1 in list_1)
                    {
                        foreach (var item2 in list_2)
                        {
                            // var abs = op.AboveOfRelaxed(item1, item2);
                            var abs = op.BelowOfRelaxed(item1, item2);
                            if (abs)
                                resCollection.Add(new Tuple<Guid, Guid>(Guid.Parse(item1.Name), Guid.Parse(item2.Name)));
                        }
                    }
                }
            }

            OutputPorts[0].Data = result;
        }

        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return new GeometryOperationNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        private List<TriangleMesh> CreateTriangleMeshes(ModelInfo modelInfo)
        {
            var elements1 = modelInfo.GetCurrentElements();
            var resList = new List<TriangleMesh>();

            foreach (var item in elements1)
            {
                var jObject = item.AttributeGroups["geometry"]["threejs"] as JObject;
                if (jObject == null) continue;

                // Transformation 
                var vertices = jObject.SelectToken("vertices").ToList().Select(value => value.Value<double>()).ToArray();
                var indices = jObject.SelectToken("faces").ToList().Select(value => value.Value<int>()).ToArray();
                // var faceSet = new IndexedFaceSet(item.Id.ToString(), indices, vertices);
                var faceSet = new IndexedFaceSet(item.Id.ToString(), indices, vertices);
                resList.Add(faceSet.CreateMesh());
            }

            return resList;
        }
    }
}
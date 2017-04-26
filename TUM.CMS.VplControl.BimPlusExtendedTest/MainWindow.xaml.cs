﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BimPlus.Client.WebControls.WPF;
using TUM.CMS.ExtendedVplControl.Nodes;
using TUM.CMS.VplControl.BimPlus.Nodes;
using TUM.CMS.VplControl.BimPlus.Ports.Input;
using TUM.CMS.VplControl.BimPlus.Ports.Output;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Nodes;
using TUM.CMS.VplControl.Utilities;
using TUM.CMS.VplControl.VCCL.Nodes;
using TUM.CMS.VplControl.Watch3D.Nodes;
using TUM.CMS.VplControl.Watch3Dx.Nodes;
using TUM.CMS.VPL.Scripting.Nodes;

namespace TUM.CMS.VplControl.BimPlusExtendedTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // Data Controller instance as a connection to the bim+ framework
        private DataController _dataController;

        // ProjectSelection
        private ProjectSelection _projSel;
        private StreamWriter _streamWriter;

        // Manage all the different SUbVplControl s
        private ExtendedVplControl.Controls.ExtendedSubVplControl _subVplControl;

        // Save an instance of the composedNode 
        private ComposedNode composedNode; 

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Observe all the nodes 
            VplControl.MainVplControl.NodeCollection.CollectionChanged += NodeCollectionOnCollectionChanged;

            // Extend the PortList 
            VplControl.InputPortArea.AddNamespaceToPortSelection("TUM.CMS.VplControl.BimPlus.Ports.Input", Assembly.GetAssembly(typeof(BimPlusProjectPort)));

            // Extend the PortList 
            VplControl.OutputPortArea.AddNamespaceToPortSelection("TUM.CMS.VplControl.BimPlus.Ports.Output", Assembly.GetAssembly(typeof(BimPlusViewerPort)));


            // Load External Node Libraries
            // VplControl.MainVplControl.ExternalNodeTypes.AddRange(
            //     ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Core.VplControl)), "TUM.CMS.VplControl.Test.Nodes")
            //         .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(ComposedNode)), "TUM.CMS.ExtendedVplControl.Nodes")
                    .ToList());


            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Watch3DNode)), "TUM.CMS.VplControl.Watch3D.Nodes")
                    .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(ProjectNode)), "TUM.CMS.VplControl.BimPlus.Nodes")
                    .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(ScriptingNode)), "TUM.CMS.VPL.Scripting.Nodes")
                    .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Watch3DxNode)), "TUM.CMS.VplControl.Watch3Dx.Nodes")
                    .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(JoinNode)), "TUM.CMS.VplControl.Relations.Nodes")
                    .ToList());

            VplControl.MainVplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof (IfcNode)), "TUM.CMS.VplControl.VCCL.Nodes")
                    .ToList());

            VplControl.MainVplControl.NodeTypeMode = NodeTypeModes.All;

            // Turn off Sensitivity
            VplControl.MainVplControl.TypeSensitive = false;

            // Connect property Grid
            // propertyGrid = new PropertyGrid
            // {
            //     AutoGenerateProperties = true,
            //     SelectedObject = VplControl
            // };

            // Connect the UI event handlers
            KeyDown += VplControl.MainVplControl.VplControl_KeyDown;
            KeyUp += VplControl.MainVplControl.VplControl_KeyUp;
        }

        private void NodeCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var nodes = sender as TrulyObservableCollection<Node>;
            foreach (var node in nodes)
            {
                node.MouseDown += NodeOnMouseDown;
            }
        }

        private void NodeOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var node = sender as Node;
            if (node == null) return;

            if (e.ClickCount == 2)
            {
                // Allow this just for the Composed Nodes
                if (node.GetType() == typeof(ComposedNode))
                {
                    composedNode = node as ComposedNode;
                    if (composedNode != null)
                    {
                        _subVplControl = new ExtendedVplControl.Controls.ExtendedSubVplControl(composedNode.portMap);
                        var subWin = new Window
                        {
                            Height = 500,
                            Width = 500,
                            WindowStartupLocation = WindowStartupLocation,
                            Content = _subVplControl,
                            WindowStyle = WindowStyle.ToolWindow,
                            Title = "Composition Node"
                        };

                        subWin.Loaded += SubWinOnLoaded;
                        subWin.Closing += SubWinOnClosing;
                        subWin.ShowDialog();
                        // Forward the node ... 
                        _subVplControl.ApplyChanges(node as ComposedNode);
                    }
                    e.Handled = true;
                }
            }
        }

        private void SubWinOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // if (composedNode.filePath != null)
            // {
            //     if (File.Exists(composedNode.filePath))
            //         _subVplControl.Deserialize(composedNode.filePath);
            // }
        }

        private void SubWinOnClosing(object sender, CancelEventArgs e)
        {
            // Serialize the contents 
            if(_subVplControl != null && composedNode != null)
                if(composedNode.filePath != null) 
                _subVplControl.SerializeNetwork(composedNode.filePath);

            // ShutDown window 
            var window = sender as Window;
            if (window == null)
                return;
            window.Closing -= SubWinOnClosing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            anim.Completed += (s, _) => window.Close();
            window.BeginAnimation(OpacityProperty, anim);
        }
    }
}
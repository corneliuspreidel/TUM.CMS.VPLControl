using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using BimPlus.Client;
using BimPlus.Client.Integration;
using BimPlus.Client.Integration.Login;
using BimPlus.Client.WebControls.WPF;
using BimPlus.Sdk.Data.UserAdministration;
using TUM.CMS.VplControl.BimPlus.Nodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Nodes;
using TUM.CMS.VplControl.Relations.Nodes;
using TUM.CMS.VplControl.Utilities;
using TUM.CMS.VplControl.Watch3D.Nodes;
using TUM.CMS.VplControl.Watch3Dx.Nodes;
using TUM.CMS.VPL.Scripting.Nodes;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TUM.CMS.VplControl.BimPlusTest_2
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
        private PropertyGrid propertyGrid;
        private StreamWriter _streamWriter;


        // Manage all the different SUbVplControl s
        private List<CMS.ExtendedVplControl.Controls.ExtendedVplControl> _subVplControls;

        private ObservableCollection<Port> startPorts;
        private ObservableCollection<Port> endPorts;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Sub VplControlss
            _subVplControls = new List<CMS.ExtendedVplControl.Controls.ExtendedVplControl>();

            // Save the Start and Endports
            startPorts = new ObservableCollection<Port>();
            endPorts = new ObservableCollection<Port>();

            var addButton = new Button();
            addButton.Content = "Add Input Port";

            // Set a global input port
            var startPort = new Port("TestPort", PortTypes.Output, typeof(object), VplControl);
            startPort.Data = 1;
            startPorts.Add(startPort);
            startPort.SizeChanged += StartPortOnSizeChanged;
            InputControl.Children.Add(startPort);

            // Set a global input port
            var startPort2 = new Port("TestPort", PortTypes.Output, typeof(object), VplControl);
            startPort2.Data = 2;
            startPorts.Add(startPort2);
            startPort2.SizeChanged += StartPortOnSizeChanged;
            InputControl.Children.Add(startPort2);

            // Set a global output port
            var endPort = new Port("TestPort", PortTypes.Input, typeof(object), VplControl);
            endPorts.Add(endPort);
            endPort.SizeChanged += EndPortOnSizeChanged;
            OutputControl.Children.Add(endPort);

            // Check when the layout of the Control has been updated 
            VplControl.LayoutUpdated += VplControlOnLayoutUpdated;

            // Observe all the nodes 
            VplControl.NodeCollection.CollectionChanged += NodeCollectionOnCollectionChanged;


            // Login ...
            Login();

            // Teams .. 
            if (_dataController != null)
            {
                TeamComboBox.ItemsSource = _dataController.IntBase.GetTeams();
                TeamComboBox.DisplayMemberPath = "DisplayName";
                TeamComboBox.SelectedItem = _dataController.IntBase.CurrentTeam;
            }

            // Load External Node Libraries
            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Test.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Watch3DNode)), "TUM.CMS.VplControl.Watch3D.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof (IfcNode)), "TUM.CMS.VplControl.IFC.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(ProjectNode)), "TUM.CMS.VplControl.BimPlus.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(ScriptingNode)), "TUM.CMS.VPL.Scripting.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(Watch3DxNode)), "TUM.CMS.VplControl.Watch3Dx.Nodes")
                    .ToList());

            VplControl.ExternalNodeTypes.AddRange(
                ClassUtility.GetTypesInNamespace(Assembly.GetAssembly(typeof(JoinNode)), "TUM.CMS.VplControl.Relations.Nodes")
                    .ToList());

            VplControl.NodeTypeMode = NodeTypeModes.All;

            // Turn off Sensitivity
            VplControl.TypeSensitive = false;

            // Connect property Grid
            propertyGrid = new PropertyGrid
            {
                AutoGenerateProperties = true,
                SelectedObject = VplControl
            };

            // Connect the UI event handlers
            KeyDown += VplControl.VplControl_KeyDown;
            KeyUp += VplControl.VplControl_KeyUp;
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
                // Create a XML Writer
                var settings = new XmlWriterSettings()
                {
                    Indent = true,
                    NewLineOnAttributes = false,
                    Encoding = new UTF8Encoding()
                };
                
                StringWriter sb = new StringWriterWithEncoding(Encoding.UTF8);
                
                using (var xmlWriter = XmlWriter.Create(sb, settings))
                {
                    node.SerializeNetwork(xmlWriter);
                }

                // Casting to ObservableCollection
                // var list = new ObservableCollection<Port>();
                // foreach (var item in node.InputPorts)
                // {
                //     list.Add(item.Clone() as Port);
                // }
                // 
                // var list_2 = new ObservableCollection<Port>();
                // foreach (var item in node.OutputPorts)
                // {
                //     list_2.Add(item.Clone() as Port);
                // }

                // Check the Input and Output
                // Get INput and Output of the node
                // var subControl = new ExtendedVplControl(list, list_2);
                // var win = new Window
                // {
                //     WindowStartupLocation = WindowStartupLocation.CenterOwner,
                //     Height = 200,
                //     Width = 200,
                //     Content = subControl
                // };
                // 
                // win.Show();
                // 
                // e.Handled = true;

            }
        }

        private void VplControlOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            // ScaleTransform due to zooming event 
            foreach (var port in startPorts)
            {
                if (port != null)
                {
                    port.Origin.X = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), VplControl).X;
                    port.Origin.Y = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), VplControl).Y;
                }
            }

            foreach (var port in endPorts)
            {

                // EndPortOnSizeChanged(port, new SizeChangedEventArgs());
                if (port != null)
                {
                    port.Origin.X = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), VplControl).X;
                    port.Origin.Y = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), VplControl).Y;
                }
            }
        }

        private void EndPortOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var endPort = sender as Port;

            if (endPort != null)
            {
                endPort.Origin.X = endPort.TranslatePoint(new Point(endPort.Width / 2, endPort.Height / 2), VplControl).X;
                endPort.Origin.Y = endPort.TranslatePoint(new Point(endPort.Width / 2, endPort.Height / 2), VplControl).Y;
            }
            sizeChangedEventArgs.Handled = true;
        }

        private void StartPortOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var startPort = sender as Port;
            if (startPort != null)
            {
                startPort.Origin.X = startPort.TranslatePoint(new Point(startPort.Width / 2, startPort.Height / 2), VplControl).X;
                startPort.Origin.Y = startPort.TranslatePoint(new Point(startPort.Width / 2, startPort.Height / 2), VplControl).Y;
            }
        }

        /// <summary>
        /// Show Project Selection Button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowProjectSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_projSel == null)
            {
                _projSel = new ProjectSelection(DataController.Instance.IntBase);
            }
        }  

        private void ShowPropertyGrid_OnClick(object sender, RoutedEventArgs e)
        {
            // Show the property grid somewhere 
        }

        public void Login()
        {
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                @"\Nemetschek\bim+\IntegrationBase.Log";
            FileStream fileStream = new FileStream(fileName, FileMode.Append);

            _streamWriter = new StreamWriter(fileStream);

            _dataController = DataController.Instance;
            _dataController.IntBase = new IntegrationBase(_streamWriter);

            // Init the data controller instance ... 
            ILoginWindow loginWindow = new DefaultLogin();

            if (_dataController.IntBase.ConnectWithLoginDialog(loginWindow))
            {
                Guid projectId = Guid.Empty;

                // Init the eventHandlers
                if (_dataController.IntBase.Connection != null && _dataController.IntBase.Connection.AccessToken() != Guid.Empty)
                {
                    _dataController.IntBase.EventHandlerCore.ProjectChanged += EventHandlerCoreOnProjectChanged;
                    _dataController.IntBase.EventHandlerCore.TeamChanged += EventHandlerCoreOnTeamChanged;
                }
            }

            // Choose the first default Team
            _dataController.IntBase.CurrentTeam = _dataController.IntBase.GetTeams().FirstOrDefault();

            // Update UI
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                    () =>
                    {
                        CurrentUserLabel.Content = _dataController.IntBase.UserName.ToString();
                    }));
        }

        private void EventHandlerCoreOnTeamChanged(object sender, BimPlusEventArgs e)
        {
            // 
        }

        /// <summary>
        /// EventHandler for Project Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void EventHandlerCoreOnProjectChanged(object sender, EventArgs eventArgs)
        {
            // if (Application.Current != null)
            //     Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
            //                 () =>
            //                 {
            //                     myDockingManager.Layout.RemoveChild(_projSelectionlayoutAnchorable);
            //                 }));

            // Get ID
            var args = eventArgs as BimPlusEventArgs;

            if (args == null) return;

            foreach (var item in DataController.Instance.IntBase.APICore.Projects.GetShortProjects())
            {
                if (item.Id != args.Id) continue;
                // Set it local
                // Update UI
                if (Application.Current != null)
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            CurrentProjectLabel.Content = item.Name;
                        }));

                break;
            }
        }

        private void TeamComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbx = sender as ComboBox;
            if (cbx != null)
            {

                var team = cbx.SelectedItem as DtoClientTeam;
                _dataController.IntBase.EventHandlerCore.OnTeamChanged(new BimPlusEventArgs() { Id = team.Id});
                // _dataController.IntBase.CurrentTeam = team;
            }
        }
    }
}
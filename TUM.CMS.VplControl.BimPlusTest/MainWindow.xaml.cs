using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BimPlus.Client;
using BimPlus.Client.Integration;
using BimPlus.Client.Integration.Login;
using BimPlus.Client.WebControls.WPF;
using BimPlus.Sdk.Data.TenantDto;
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
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace TUM.CMS.VplControl.BimPlusTest
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

        // UI Elements
        private LayoutAnchorablePane anchorablePane;
        private LayoutAnchorable layoutAnchorable;
        private LayoutAnchorable _projSelectionlayoutAnchorable;

        private PropertyGrid propertyGrid;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

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

        /// <summary>
        /// Show Project Selection Button clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowProjectSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if(_projSelectionlayoutAnchorable == null)
                _projSelectionlayoutAnchorable = new LayoutAnchorable { Title = "Project Selection" };

            if (_projSel == null)
            {
                _projSel = new ProjectSelection(DataController.Instance.IntBase);
                _projSelectionlayoutAnchorable.Content = _projSel;
                _projSelectionlayoutAnchorable.AddToLayout(myDockingManager, AnchorableShowStrategy.Left);
            }

            if (_projSelectionlayoutAnchorable.IsVisible)
            {
                _projSelectionlayoutAnchorable.Hide();
            }
            else
            {
                _projSelectionlayoutAnchorable.Show();
            }
        }  

        

        private void ShowPropertyGrid_OnClick(object sender, RoutedEventArgs e)
        {
            if (anchorablePane == null)
            {
                anchorablePane = new LayoutAnchorablePane();
                if (layoutAnchorable == null)
                {
                    layoutAnchorable = new LayoutAnchorable
                    {
                        Title = "PropertyGrid",
                        Content = propertyGrid
                    };
                    anchorablePane.Children.Add(layoutAnchorable);
                }
            }
            layoutAnchorable.AddToLayout(myDockingManager, AnchorableShowStrategy.Left);
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
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(
                            () =>
                            {
                                myDockingManager.Layout.RemoveChild(_projSelectionlayoutAnchorable);
                            }));

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
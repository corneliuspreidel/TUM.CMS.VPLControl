using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Client;
using BimPlus.Client.Integration;
using BimPlus.Client.Integration.Login;
using BimPlus.Client.WebControls.WPF;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Ports.Input
{
    public class BimPlusProjectPort : ExtendedPort
    {
        private StreamWriter _streamWriter;
        private DataController _dataController;
        private ProjectSelection _projectSelection;
        private Window hostWindow;
        private TextBlock textBlock; 

        private Grid contentGrid; 

        public BimPlusProjectPort(string name, PortTypes portType, Type type, Core.VplControl hostCanvas)
            : base(name, portType, type, hostCanvas)
        {
            _dataController = DataController.Instance;

            // AddPopupContent(contentGrid);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            textBlock = new TextBlock() { MinWidth = 120, MaxWidth = 300, IsHitTestVisible = false };

            var button = new Button
            {
                Content = "ProjectSelection",
                Margin = new Thickness(5)
            };

            button.Click += ButtonOnClick;


            grid.Children.Add(textBlock);
            grid.Children.Add(button);

            AddPopupContent(grid);

            // MouseDown += OnMouseDown;
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_dataController.IntBase == null)
                Login();

            if (_dataController.IntBase == null)
                return;

            _dataController.IntBase.EventHandlerCore.ProjectChanged += EventHandlerCoreOnProjectChanged;

            _projectSelection = new ProjectSelection(_dataController.IntBase);

            contentGrid = new Grid()
            {
                Height = 600,
                Width = 500
            };

            contentGrid.Children.Add(_projectSelection);

            hostWindow = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight
            };

            hostWindow.Content = contentGrid;
            hostWindow.ShowDialog();
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
                if (_dataController.IntBase.ApiCore.AccessToken() != Guid.Empty)
                {
                    _dataController.IntBase.EventHandlerCore.ProjectChanged += EventHandlerCoreOnProjectChanged;
                    _dataController.IntBase.EventHandlerCore.TeamChanged += EventHandlerCoreOnTeamChanged;
                }

                // Init the eventHandlers
                // _dataController.IntBase.ApiCo
                // _dataController.IntBase.
                // if (_dataController.IntBase.ApiCore.Connection != null && _dataController.IntBase.Connection.AccessToken() != Guid.Empty)
                // {
                //     _dataController.IntBase.EventHandlerCore.ProjectChanged += EventHandlerCoreOnProjectChanged;
                //     _dataController.IntBase.EventHandlerCore.TeamChanged += EventHandlerCoreOnTeamChanged;
                // }
            }

            // Choose the first default Team
            
            _dataController.IntBase.CurrentTeam = _dataController.IntBase.ApiCore.GetTeams().FirstOrDefault();
        }

        private void EventHandlerCoreOnTeamChanged(object sender, BimPlusEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void EventHandlerCoreOnProjectChanged(object sender, BimPlusEventArgs e)
        {
            if (e == null)
                return;
            var id = e.Id;
            if (id == null)
                return;

            foreach (var project in _dataController.IntBase.ApiCore.Projects.GetShortProjects())
            {
                if (project.Id == id)
                {
                    Data = project;
                    textBlock.Text = project.Name;
                }
            }

            // var project = _dataController.IntBase.APICore.Projects.GetProject(id);
            // Data = project;

            if (hostWindow != null)
                if (hostWindow.IsActive)
                    hostWindow.Close();
        }
    }
}

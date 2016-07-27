using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Client;
using BimPlus.Client.WebControls.WPF;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class WebProjectSelectionNode : UtilityNode
    {
        // DataController
        private readonly DataController _controller;

        public WebProjectSelectionNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            AddOutputPortToNode("Project", typeof(object));
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            IsResizeable = true;

            var webControl = new ProjectSelection(_controller.IntBase);
            {
                Visibility = Visibility.Visible;
            };

            // webControl.ProjectChangedEventHandler += ProjectSelectionViewModelOnProjectChangedEventHandler;
            // webControl.TeamChangedEventHandler += ProjectSelectionViewModelOnTeamChangedEventHandler;

            var pr = new ContentPresenter
            {
                Content = webControl,
                MinWidth = 600,
                MinHeight = 600
            };

            AddControlToNode(pr);
        }

        private void ProjectSelectionViewModelOnTeamChangedEventHandler(object sender, EventArgs eventArgs)
        {
            // Raised if the Team is changed
        }

        private void ProjectSelectionViewModelOnProjectChangedEventHandler(object sender, EventArgs eventArgs)
        {
            foreach (var proj in _controller.IntBase.APICore.Projects.GetShortProjects().Where(proj => proj.Id == (eventArgs as BimPlusEventArgs).Id))
            {
                OutputPorts[0].Data = proj;
            }
        }

        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return new WebProjectSelectionNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
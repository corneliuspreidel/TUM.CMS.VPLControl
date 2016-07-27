using System.Collections.ObjectModel;
using BimPlus.Sdk.Data.DbCore.Structure;
using BimPlus.Sdk.Data.TenantDto;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class IssueContainerNode : UtilityNode
    {
        private readonly DataController _controller;
        private ObservableCollection<DtoShortIssue> _issues;  

        public IssueContainerNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            AddInputPortToNode("Project", typeof(object));
            AddOutputPortToNode("Issues", typeof(object));
            DataContext = this;
        }

        public override void Calculate()
        {
            if (InputPorts[0] == null) return;

            if (InputPorts[0].Data.GetType() != typeof (Project)) return;

            var project = InputPorts[0].Data as Project;
            if (project == null) return;

            foreach (var item in _controller.IntBase.APICore.Issues.GetShortIssues(project.Id))
            {
                _issues.Add(item);
            }
            
            OutputPorts[0].Data = _issues;
        }

        public override Node Clone()
        {
            return new IssueContainerNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
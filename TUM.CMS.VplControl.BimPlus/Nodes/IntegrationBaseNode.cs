using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class IntegrationBaseNode : UtilityNode
    {
        private DataController _controller;

        public IntegrationBaseNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddOutputPortToNode("IntegrationBase", typeof (object));
            DataContext = this;

            var lbl = new System.Windows.Controls.Label {Content = "IntegrationBase"};
            AddControlToNode(lbl);

            Calculate();
        }

        public override void Calculate()
        {
            _controller = DataController.Instance;

            if (_controller.IntBase != null)
            {
                OutputPorts[0].Data = _controller.IntBase;
            }
        }

        public override Node Clone()
        {
            return new IssueNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
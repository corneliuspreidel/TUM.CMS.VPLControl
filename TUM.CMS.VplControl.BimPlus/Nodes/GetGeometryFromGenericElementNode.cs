using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using BimPlus.IntegrationFramework.Contract.Model;
using BimPlus.Sdk.Data.Geometry;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class GetGeometryFromGenericElementNode : Node
    {
        private readonly DataController _controller;

        public GetGeometryFromGenericElementNode(Core.VplControl hostCanvas): base(hostCanvas)
        {
            AddInputPortToNode("InputElements", typeof (List<GenericElement>));
            AddOutputPortToNode("ConvertedElements", typeof(List<DbGeometry>));

            _controller = DataController.Instance;

            DataContext = this;
        }

        public override void Calculate()
        {
            // Check values ... 
            if (InputPorts[0].Data == null || InputPorts[1].Data == null) return;

            var genericElements = InputPorts[0].Data as List<GenericElement>;
            if (genericElements == null) return;

            var res = new List<DbGeometry>();

            foreach (var item in genericElements)
            {
                res.AddRange(_controller.IntBase.APICore.GetElementGeometryAsDbGeometry(item.Id));
            }

            // Clear the list once again ... 
            res.RemoveAll(null);
            OutputPorts[0].Data = res;
        }

        public override Node Clone()
        {
            return new ElementFilterNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Windows;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.VCCL.Nodes
{
    public class IFCNode: Node
    {
        public IFCNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // AddInputPortToNode("Relation", typeof(Relation));
            // AddInputPortToNode("Relation", typeof(Relation));
            // 
            // AddOutputPortToNode("Relation3", typeof(Relation3));
            // BinButton.Visibility = Visibility.Hidden;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;
            if (InputPorts[1].Data == null)
                return;

          
            OutputPorts[0].Data = null;
        }

        public override Node Clone()
        {
            return new IFCNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
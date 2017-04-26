using System;
using System.Collections.ObjectModel;
using System.Windows;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Data;

namespace TUM.CMS.VplControl.Relations.Nodes
{
    public class JoinNode: RelationalObjectNode
    {
        public JoinNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Relation", typeof(Relation));
            AddInputPortToNode("Relation", typeof(Relation));

            AddOutputPortToNode("Relation3", typeof(Relation3));
            BinButton.Visibility = Visibility.Hidden;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;
            if (InputPorts[1].Data == null)
                return;

            var relation1 = InputPorts[0]?.Data as Relation;
            var relation2 = InputPorts[1]?.Data as Relation;

            var rel1Collection = relation1?.Collection as ObservableCollection<Tuple<Guid, Guid>>;
            var rel2Collection = relation2?.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            var resultRelation = new Relation3(relation1.ModelId, relation1.ProjectId);
            var resCollection = resultRelation.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;

            foreach (var item in rel1Collection)
            {
                foreach (var item2 in rel2Collection)
                {
                    if (item.Item2 == item2.Item1)
                    {
                        resCollection?.Add(new Tuple<Guid, Guid, Guid>(item.Item1, item.Item2, item2.Item2));
                    }
                }
            }

            OutputPorts[0].Data = resultRelation;
        }

        public override Node Clone()
        {
            return new JoinNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
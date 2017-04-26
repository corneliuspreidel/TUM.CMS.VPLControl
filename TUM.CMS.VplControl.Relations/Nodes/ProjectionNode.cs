using System;
using System.Collections.ObjectModel;
using System.Windows;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Data;

namespace TUM.CMS.VplControl.Relations.Nodes
{
    public class ProjectionNode : RelationalObjectNode
    {
        public ProjectionNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Relation3", typeof(Relation3));
            AddOutputPortToNode("Relation", typeof(Relation));
            BinButton.Visibility = Visibility.Hidden;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;
            var relation = InputPorts[0]?.Data as Relation3;
            var relCollection = relation.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;
            var resRelation = new Relation(relation.ModelId, relation.ProjectId);

            var resCollection = resRelation.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            foreach (var item in relCollection)
            {
                resCollection?.Add(new Tuple<Guid, Guid>(item.Item1, item.Item3));
            }

            OutputPorts[0].Data = resRelation;
        }

        public override Node Clone()
        {
            return new ProjectionNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
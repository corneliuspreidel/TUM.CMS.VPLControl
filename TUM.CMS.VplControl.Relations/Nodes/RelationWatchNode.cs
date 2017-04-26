using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Controls;
using TUM.CMS.VplControl.Relations.Data;

namespace TUM.CMS.VplControl.Relations.Nodes
{
    public class RelationWatchNode : RelationalObjectNode
    {
        private RelationNodeControl nodeControl;
        private BaseRelation _relation;

        public RelationWatchNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            AddInputPortToNode("Input", typeof(BaseRelation));
            AddOutputPortToNode("Selected", typeof (object));

            nodeControl = new RelationNodeControl
            {
                Margin = new Thickness(5, 20, 5, 15)
            };

            nodeControl.ListBox.SelectionChanged += ListBoxOnSelectionChanged;

            AddControlToNode(nodeControl);
        }


        public override void Calculate()
        {
            // Input Part
            if (InputPorts[0].Data.GetType() != typeof (Relation)) return;

            _relation = InputPorts[0].Data as Relation;

            if (_relation == null)
                return;

            nodeControl.ListBox.Items.Clear();
            foreach (var item in _relation.Collection)
            {
                nodeControl.ListBox.Items.Add(item);
            }
            BinButton.Visibility = Visibility.Visible;
            OutputPorts[0].Data = _relation;
        }

        public override Node Clone()
        {
            return new RelationWatchNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        private void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (_relation == null)
                return;

            var res = new Relation(_relation.ModelId, _relation.ProjectId);
            var collection = res.Collection as ObservableCollection<Tuple<Guid, Guid>>;

            foreach (var item in nodeControl.ListBox.SelectedItems)
            {
                collection.Add(item as Tuple<Guid, Guid>);
            }
            OutputPorts[0].Data = res;
        }
    }
}
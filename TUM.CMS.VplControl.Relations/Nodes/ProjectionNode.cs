using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Controls;
using TUM.CMS.VplControl.Relations.Data;
using TUM.CMS.VplControl.Utilities;
using Xceed.Wpf.Toolkit;

namespace TUM.CMS.VplControl.Relations.Nodes
{
    public class ProjectionNode : RelationalObjectNode
    {
        private ProjectionNodeControl control;
        private BaseRelation relation;

        public ProjectionNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            AddInputPortToNode("Relation3", typeof(Relation3));
            AddOutputPortToNode("Relation", typeof(Relation));
            BinButton.Visibility = Visibility.Hidden;

            control = new ProjectionNodeControl();
            control.projectSelectionComboBox.SelectionChanged += ProjectSelectionComboBoxOnSelectionChanged;

            AddControlToNode(control);

        }

        private void ProjectSelectionComboBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var item = (sender as WatermarkComboBox)?.SelectedItem;
            if (item == null)
                return;
            
            // prepare the Result
            var result = new ModelInfo {modelId = relation.ModelId.ToString()};

            if (relation.GetType() == typeof(Relation))
            {
                var relation2 = InputPorts[0]?.Data as Relation;
                if (relation2 == null)
                    return;

                foreach (var elem in relation2.Collection as ObservableCollection<Tuple<Guid, Guid>>)
                {
                    switch ((item as string))
                    {
                        case "1":
                            result.elementIds.Add(elem.Item1.ToString());
                            break;
                        case "2":
                            result.elementIds.Add(elem.Item2.ToString());
                            break;
                    }
                }
            }

            OutputPorts[0].Data = result;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null)
                return;

            relation = InputPorts[0].Data as BaseRelation;

            if (relation == null)
                return; 

            // Init the ComboBox
            control.projectSelectionComboBox.Items.Clear();

            if (relation.GetType() == typeof(Relation))
            {
                // var relation2 = InputPorts[0]?.Data as Relation;
                control.projectSelectionComboBox.Items.Add("1");
                control.projectSelectionComboBox.Items.Add("2");
            }

            else if (relation.GetType() == typeof(Relation3))
            {
                var relation3 = InputPorts[0]?.Data as Relation3;
                var relCollection = relation3.Collection as ObservableCollection<Tuple<Guid, Guid, Guid>>;
                var resRelation = new Relation(relation3.ModelId, relation3.ProjectId);

                var resCollection = resRelation.Collection as ObservableCollection<Tuple<Guid, Guid>>;

                foreach (var item in relCollection)
                {
                    resCollection?.Add(new Tuple<Guid, Guid>(item.Item1, item.Item3));
                }

                OutputPorts[0].Data = resRelation;
            }
           
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
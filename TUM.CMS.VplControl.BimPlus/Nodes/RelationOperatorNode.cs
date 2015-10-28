using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BimPlus.IntegrationFramework.Contract.Model;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class RelationOperatorNode : Node
    {
        private ObservableCollection<Tuple<object, object>> _relationElements;
        private ComboBox combo { get; set; }

        public RelationOperatorNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Input Declaration
            AddInputPortToNode("Relation_1", typeof(Tuple<object, object>));
            AddOutputPortToNode("Relation_2", typeof (Tuple<object, object>));

            combo = new ComboBox();

            combo.Items.Add(new ComboBoxItem { Content = "JOIN" });
            combo.Items.Add(new ComboBoxItem { Content = "etc." });

            combo.SelectedIndex = 0;
            AddControlToNode(combo);
        }

        public override void Calculate()
        {
            // Input Part
            if (InputPorts[0].Data.GetType() != typeof (Project)) return;
            // _modelComboBox.ItemsSource = null;

            var project = InputPorts[0].Data as Project;
            if (project == null) return;

            // Output Part
            // if (_modelComboBox != null) OutputPorts[0].Data = _modelComboBox.SelectedItem as Division;
        }

        public override Node Clone()
        {
            return new RelationNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

        #region PropertyChangedHandlers
        public ObservableCollection<Tuple<object, object>> RelationElements
        {
            get { return _relationElements; }
            set
            {
                _relationElements = value;
                OnPropertyChanged("RelationElements");
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event 
        private new void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion PropertyChangedHandlers
    }
}
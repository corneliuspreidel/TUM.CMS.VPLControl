using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BimPlus.Sdk.Data.DbCore;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class ElementMergeNode : Node
    {
        private readonly DataController _controller;

        public ElementMergeNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            var lbl = new Label{Content = "Filter Node ", Margin = new Thickness(5,15, 5, 10)};
            AddInputPortToNode("Input", typeof (object));
            AddInputPortToNode("Input", typeof(object));

            AddOutputPortToNode("MergedElements", typeof(object));

            // LabelCaption.Visibility = Visibility.Visible;
            // LabelCaption.Content = "";
            DataContext = this;
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null || InputPorts[1].Data == null)
                return;

            var mergedGenericElements = new List<DtObject>();

            if (InputPorts[0].Data.GetType() == typeof (List<DtObject>) &&
                InputPorts[1].Data.GetType() == typeof (List<DtObject>))
            {
                mergedGenericElements.AddRange(InputPorts[0].Data as List<DtObject>);
                mergedGenericElements.AddRange(InputPorts[1].Data as List<DtObject>);
            }

            OutputPorts[0].Data = mergedGenericElements;
        }

        public override Node Clone()
        {
            return new ElementMergeNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
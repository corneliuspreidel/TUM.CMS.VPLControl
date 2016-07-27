using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Nodes;
using TUM.CMS.VplControl.Relations.Controls;
using TUM.CMS.VplControl.Relations.Data;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class BimPlusWatchNode : WatchNode
    {
        private RelationNodeControl nodeControl;
        private Relation _relation;

        public BimPlusWatchNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 120,
                MinHeight = 20,
                CanContentScroll = true,
                Content = textBlock,
            };

            AddControlToNode(scrollViewer);
        }


        public override void Calculate()
        {
            if (InputPorts[0] == null || ControlElements[0] == null) return;

            var scrollViewer = ControlElements[0] as ScrollViewer;
            if (scrollViewer == null) return;

            var textBlock = scrollViewer.Content as TextBlock;
            if (textBlock == null) return;

            if (InputPorts[0].Data == null)
                textBlock.Text = "null";

            if (InputPorts[0].Data.GetType() == typeof(Relation))
            {
                
            }
            else
            {
                base.Calculate();
            }
        }

        public override Node Clone()
        {
            return new BimPlusWatchNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
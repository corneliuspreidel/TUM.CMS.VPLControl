using System.Windows;
using System.Windows.Media;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Relations.Nodes
{ 
    public abstract class RelationalObjectNode : Node
    {
        protected RelationalObjectNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            if (Border == null) return;

            Border.BorderBrush = Brushes.BlueViolet;
            Border.BorderThickness = new Thickness(2);
            Border.CornerRadius = new CornerRadius(0);
        }
    }
}
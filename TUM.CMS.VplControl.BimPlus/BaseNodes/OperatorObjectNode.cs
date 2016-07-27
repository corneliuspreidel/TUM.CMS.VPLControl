using System.Windows;
using System.Windows.Media;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.BaseNodes
{ 
    public abstract class OperatorObjectNode : Node
    {
        protected OperatorObjectNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            if (Border == null) return;

            Border.BorderBrush = Brushes.Red;
            Border.BorderThickness = new Thickness(2);
            Border.CornerRadius = new CornerRadius(20);
        }
    }
}
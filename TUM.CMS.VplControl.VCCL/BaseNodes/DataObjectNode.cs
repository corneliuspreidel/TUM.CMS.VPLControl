using System.Windows;
using System.Windows.Media;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.BaseNodes
{ 
    public abstract class DataObjectNode : Node
    {
        protected DataObjectNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {

            if (Border == null) return;
            Border.BorderBrush = Brushes.Blue;
            Border.BorderThickness = new Thickness(2);
            Border.CornerRadius = new CornerRadius(0);
        }
    }
}
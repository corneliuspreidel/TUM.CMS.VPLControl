using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Controls;
using TUM.CMS.ExtendedVplControl.Nodes;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports
{
    public class ExtendedPort: Port
    {
        private Popup popup;
        private Grid PopupGrid;
        private Border PopupBorder;
        private Guid Id;

        public ExtendedPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas, Guid id = new Guid())
            : base(name, portType, type, hostCanvas)
        {
            // DefaultNode for the handling in the mainHostCanvas --> Checking the parentNode which should not be null
            ParentNode = new DefaultNode(hostCanvas);

            // Check (Create a Guid for the Port)
            if (id == Guid.Empty)
                Id = Guid.NewGuid(); 

            // UI 
            PopupGrid = new Grid
            {
                Background = Brushes.Transparent,
                Opacity = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(2)
            };

            PopupBorder = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(5),
                BorderBrush = new SolidColorBrush(Colors.DarkGray),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10)
            };

            // Then add your border to the grid
            PopupGrid.Children.Add(PopupBorder);

            popup = new Popup
            {
                Child = PopupGrid,
                PlacementTarget = this,
                PopupAnimation = PopupAnimation.Slide,
                AllowsTransparency = true,
                ClipToBounds = true,
            };

            // Turn off the Toolips
            ToolTip = null;

            MouseDown += OnMouseDown;

            MouseEnter += PortOnMouseEnter;
            MouseLeave += PortOnMouseLeave;
            PopupGrid.MouseLeave += PopupGridOnMouseLeave;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                var port = sender as ExtendedPort;
                if (port == null)
                    return;

                // Get the ExtendedVplControl as Host UserControl 
                DependencyObject ucParent = Parent;

                while (!(ucParent is PortArea))
                {
                    ucParent = LogicalTreeHelper.GetParent(ucParent);
                }
                var portArea = ucParent as PortArea;
                if (portArea == null)
                    return;

                try
                {
                    // Remove Connections, the port itself and recompute the port positions ... 
                    foreach (var item in port.ConnectedConnectors)
                    {
                        item.RemoveFromCanvas();
                    }

                    port.UpdateLayout();
                    portArea.PortControl.Children.Remove(port);
                    portArea.RecalculateLocationForAllPorts();
                }
                catch (Exception)
                {

                }

                e.Handled = true; 
            }
        }

        private void PopupGridOnMouseLeave(object sender, MouseEventArgs e)
        {
            // Popup should stay for a while opened

            // popup.Dispatcher.Thread.Sleep(500);
            // popup.Dispatcher.BeginInvoke(new Action(() =>
            // {
            //     System.Threading.Thread.Sleep(750);
            // }));

            popup.IsOpen = false;
            e.Handled = true;
        }

        protected void AddPopupContent(UIElement element)
        {
            PopupGrid.Children.Add(element);
        }

        private void PortOnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!PopupGrid.IsMouseOver)
            {
                popup.IsOpen = false;
            }
                
            e.Handled = true;
        }

        private void PortOnMouseEnter(object sender, MouseEventArgs e)
        {
            // OutpurPorts
            if (PortType == PortTypes.Input)
            {
                // popup.VerticalOffset =- ActualHeight;
                // popup.HorizontalOffset =+ ActualWidth;
            }
            // Input Ports
            else if (PortType == PortTypes.Output)
            {
                popup.VerticalOffset =- ActualHeight;
                popup.HorizontalOffset =  + ActualWidth;
            }
           
            popup.IsOpen = true;
            
            e.Handled = true;
        }

        public void SetAllowTransparencyForPopup(bool setting)
        {
            popup.AllowsTransparency = setting; 
        }

        public virtual void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Id");
            xmlWriter.WriteValue(Id.ToString());
            xmlWriter.WriteEndAttribute();
            xmlWriter.WriteStartAttribute("PortType");
            xmlWriter.WriteValue(PortType.ToString());
            xmlWriter.WriteEndAttribute();
            xmlWriter.WriteStartAttribute("ParentNodeId");
            xmlWriter.WriteValue(ParentNode.Guid.ToString());
            xmlWriter.WriteEndAttribute();
        }

        public virtual void DeserializeNetwork(XmlReader xmlReader)
        {
            var value = xmlReader.GetAttribute("Id");
            if (value != null)
                Data = Guid.Parse(value);

            value = xmlReader.GetAttribute("PortType");
            if (value != null)
                PortType = (PortTypes)Enum.Parse(typeof(PortTypes), value);

            value = xmlReader.GetAttribute("ParentNodeId");
            if (value != null)
                ParentNode.Guid = Guid.Parse(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Controls
{
    /// <summary>
    /// Interaction logic for PortArea.xaml
    /// </summary>
    public partial class PortArea : UserControl
    {
        // private PortAreaType portAreaType;
        
        // Dependency Property
        public static readonly DependencyProperty portAreaTypeProperty =
             DependencyProperty.Register("PortAreaType", typeof(PortAreaType), typeof(PortArea), new FrameworkPropertyMetadata(null));

        // .NET Property wrapper
        public PortAreaType PortAreaType
        {
            get { return (PortAreaType)GetValue(portAreaTypeProperty); }
            set { SetValue(portAreaTypeProperty, value); }
        }

        private PortSelection portSelection;
        private ExtendedVplControl _extendedVplControl;

        public PortArea()
        {
            InitializeComponent();
            portSelection = new PortSelection {PlacementTarget = this};
            portSelection.PortList.SelectionChanged += PortListOnSelectionChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Get the ExtendedVplControl as Host UserControl 
            DependencyObject ucParent = Parent;

            while (!(ucParent is UserControl))
            {
                ucParent = LogicalTreeHelper.GetParent(ucParent);
            }

            _extendedVplControl = ucParent as ExtendedVplControl;
            _extendedVplControl.MainVplControl.LayoutUpdated += MainVplControlOnLayoutUpdated;

            IEnumerable<Type> types = null;

            // Set Input or Output Nodes for the Selection 
            if (PortAreaType == PortAreaType.InputArea)
            {
                AddNamespaceToPortSelection("TUM.CMS.ExtendedVplControl.Ports.Input", Assembly.GetAssembly(GetType()));
            }
            else if (PortAreaType == PortAreaType.OutputArea)
            {
                AddNamespaceToPortSelection("TUM.CMS.ExtendedVplControl.Ports.Output", Assembly.GetAssembly(GetType()));
            }
        }

        public void AddNamespaceToPortSelection(string Namespace, Assembly assembly)
        {
            var types = from t in assembly.GetTypes()
                    where t.IsClass && t.Namespace == Namespace
                    select t;

            foreach (var type in types)
            {
                portSelection.PortTypes.Add(type);
            }
        }

        public void AddPort(Port port)
        {
            port.SizeChanged += PortOnSizeChanged;
            PortControl.Children.Add(port);

            // For handling as SubControl
            if(_extendedVplControl._extendedPortMap != null)
                _extendedVplControl._extendedPortMap.Add(Guid.NewGuid(), port as ExtendedPort);
        }

        private void PortOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var port = sender as Port;
            ReCalculateLocation(port);
        }

        public static void ReCalculateLocation(Port port)
        {
            if (port != null)
            {
                port.Origin.X = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), port.HostCanvas).X;
                port.Origin.Y = port.TranslatePoint(new Point(port.Width / 2, port.Height / 2), port.HostCanvas).Y;
            }
        }

        public void RecalculateLocationForAllPorts()
        {
            foreach (var item in PortControl.Children)
            {
                if (item.GetType() == typeof(Port) || item.GetType().IsSubclassOf(typeof(Port)))
                {
                    ReCalculateLocation(item as Port);
                }
            }
        }

        private void MainControl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (portSelection != null)
                {
                    var current = e.GetPosition(this);
                    portSelection.HorizontalOffset = current.X;
                    portSelection.VerticalOffset = current.Y;
                    portSelection.IsOpen = true;
                }
            }
            e.Handled = true; 
        }

        private void PortListOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var listBox = sender as ListBox;
            if (listBox == null)
                return;

            Port instance = null;

            if (listBox.SelectedItem == null) return;

            switch (PortAreaType)
            {
                case PortAreaType.InputArea:
                    instance = Activator.CreateInstance((Type)listBox.SelectedItem, listBox.SelectedItem.ToString(), PortTypes.Output, typeof(object), _extendedVplControl.MainVplControl) as Port;
                    break;
                case PortAreaType.OutputArea:
                    instance = Activator.CreateInstance((Type)listBox.SelectedItem, listBox.SelectedItem.ToString(), PortTypes.Input, typeof(object), _extendedVplControl.MainVplControl) as Port;
                    break;
            }

            if (instance == null) return; 

            AddPort(instance);
            listBox.UnselectAll();
            portSelection.IsOpen = false;
        }

        private void MainVplControlOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            RecalculateLocationForAllPorts();
        }
    }

    public enum PortAreaType
    {
        InputArea,
        OutputArea
    }
}

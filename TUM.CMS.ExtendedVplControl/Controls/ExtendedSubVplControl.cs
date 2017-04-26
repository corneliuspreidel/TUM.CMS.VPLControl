
using System;
using System.Collections.Generic;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Nodes;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Controls
{
    public class ExtendedSubVplControl: ExtendedVplControl
    {
        // Stoered Information Process 
        private String filePath;

        public ExtendedSubVplControl(Dictionary<Guid, Port> portMap)
        {
            InitializeComponent();
            _extendedPortMap = new Dictionary<Guid, ExtendedPort>();

            // First Try 
            if (filePath != null)
                Deserialize(filePath);

            ExtendedPort newPort;

            // Setup the ports for the new environment
            foreach (var port in portMap)
            {
                if (port.Value.PortType == PortTypes.Input)
                {
                    if (port.Value.DataType == typeof(string))
                    {
                        newPort = new Ports.Input.StringPort("StringPort", PortTypes.Output, typeof(string),
                            MainVplControl) {Data = port.Value.Data};
                        InputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(int))
                    {
                        newPort = new Ports.Input.IntegerPort("IntegerPort", PortTypes.Output, typeof(int),
                            MainVplControl)
                        {Data = port.Value.Data};
                        InputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(double))
                    {
                        newPort = new Ports.Input.DoublePort("DoublePort", PortTypes.Output, typeof(double),
                            MainVplControl)
                        {Data = port.Value.Data};
                        InputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(object))
                    {
                        newPort = new Ports.Input.ObjectPort("ObjectPort", PortTypes.Output, typeof(object),
                            MainVplControl)
                        {Data = port.Value.Data};
                        InputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                }
                else if (port.Value.PortType == PortTypes.Output)
                {
                    if (port.Value.DataType == typeof(string))
                    {
                        newPort = new Ports.Output.StringPort("StringPort", PortTypes.Input, typeof(string),
                            MainVplControl)
                        {Data = port.Value.Data};
                        OutputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(int))
                    {
                        newPort = new Ports.Output.IntegerPort("IntegerPort", PortTypes.Input, typeof(int),
                            MainVplControl)
                        {Data = port.Value.Data};
                        OutputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(double))
                    {
                        newPort = new Ports.Output.DoublePort("DoublePort", PortTypes.Input, typeof(double),
                            MainVplControl)
                        {Data = port.Value.Data};
                        OutputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                    else if (port.Value.DataType == typeof(object))
                    {
                        newPort = new Ports.Output.ObjectPort("ObjectPort", PortTypes.Input, typeof(object),
                            MainVplControl)
                        {Data = port.Value.Data};
                        OutputPortArea.PortControl.Children.Add(newPort);
                        _extendedPortMap.Add(port.Key, newPort);
                    }
                }
            }
        }

        public void ApplyChanges(ComposedNode node)
        {
            // Serialize the processing 
            var path = @"C:\Users\Cornelius\Desktop\Test.vplxml";
            SerializeNetwork(path);

            foreach (var item in _extendedPortMap)
            {
                if (node.portMap.ContainsKey(item.Key)) continue;

                if (item.Value.PortType == PortTypes.Output)
                {
                    if (item.Value.DataType == typeof(int))
                    {
                        node.AddInputPortToNode("Integer", typeof(int));
                        node.InputPorts[node.InputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(double))
                    {
                        node.AddInputPortToNode("Double", typeof(double));
                        node.InputPorts[node.InputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(string))
                    {
                        node.AddInputPortToNode("String", typeof(string));
                        node.InputPorts[node.InputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(object))
                    {
                        node.AddInputPortToNode("Object", typeof(object));
                        node.InputPorts[node.InputPorts.Count - 1].Data = item.Value.Data;
                    }
                }
                else if (item.Value.PortType == PortTypes.Input)
                {
                    if (item.Value.DataType == typeof(int))
                    {
                        node.AddOutputPortToNode("Integer", typeof(int));
                        node.OutputPorts[node.OutputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(double))
                    {
                        node.AddOutputPortToNode("Double", typeof(double));
                        node.OutputPorts[node.OutputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(string))
                    {
                        node.AddOutputPortToNode("String", typeof(string));
                        node.OutputPorts[node.OutputPorts.Count - 1].Data = item.Value.Data;
                    }
                    else if (item.Value.DataType == typeof(object))
                    {
                        node.AddOutputPortToNode("Object", typeof(object));
                        node.OutputPorts[node.OutputPorts.Count - 1].Data = item.Value.Data;
                    }
                }
            }
        }
    }
}

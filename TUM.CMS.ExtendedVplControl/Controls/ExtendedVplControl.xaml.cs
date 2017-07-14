using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.ContentMenu;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Utilities;

namespace TUM.CMS.ExtendedVplControl.Controls
{
    /// <summary>
    /// Interaction logic for ExtendedvplControl.xaml
    /// </summary>
    public partial class ExtendedVplControl 
    {
        public Dictionary<Guid, ExtendedPort> _extendedPortMap;

        public ExtendedVplControl()
        {
            InitializeComponent();
        }

        public void SerializeNetwork(String filepath)
        {
           var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = false,
                Encoding = new UTF8Encoding()
            };

            StringWriter sb = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sb, settings))
            {
                xmlWriter.WriteStartDocument();

                xmlWriter.WriteStartElement("Document");

                xmlWriter.WriteStartAttribute("GraphFlowDirection");
                xmlWriter.WriteValue(MainVplControl.GraphFlowDirection.ToString());
                xmlWriter.WriteEndAttribute();

                xmlWriter.WriteStartElement("Nodes");

                foreach (var node in MainVplControl.NodeCollection)
                {
                    xmlWriter.WriteStartElement(node.GetType().ToString());
                    node.SerializeNetwork(xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("InputPorts");
                foreach (var port in InputPortArea.PortControl.Children)
                {
                    xmlWriter.WriteStartElement(((ExtendedPort)port)?.GetType().ToString());
                    ((ExtendedPort)port)?.SerializeNetwork(xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("OutputPorts");
                foreach (var port in OutputPortArea.PortControl.Children)
                {
                    xmlWriter.WriteStartElement(((ExtendedPort)port)?.GetType().ToString());
                    ((ExtendedPort)port)?.SerializeNetwork(xmlWriter);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Connectors");
                foreach (var connector in MainVplControl.ConnectorCollection)
                {
                    xmlWriter.WriteStartElement(connector.GetType().ToString());
                    connector.SerializeNetwork(xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();
            }

            File.WriteAllText(filepath, sb.ToString());
        }

        public void Deserialize(String filePath)
        {
            if (!File.Exists(filePath))
                return;

            if (new FileInfo(filePath).Length == 0)
                return;

            MainVplControl.NewFile();
            InputPortArea.PortControl.Children.Clear();
            OutputPortArea.PortControl.Children.Clear();

            // Create an reader
            using (var reader = new XmlTextReader(filePath))
            {
                reader.MoveToContent();

                var enumString = reader.GetAttribute("GraphFlowDirection");

                if (enumString != null)
                {
                    MainVplControl.ImportFlowDirection =
                        (GraphFlowDirections)Enum.Parse(typeof(GraphFlowDirections), enumString, true);
                }

                // reader.ReadToDescendant("Nodes");

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            if (reader.Name != null)
                            {
                                var type = Type.GetType(reader.Name);

                                if (type == null)
                                {
                                    try // try to find type in entry assembly
                                    {
                                        var assembly = Assembly.GetAssembly(typeof(VplControl.Core.VplControl));
                                        type = assembly.GetTypes().First(t => t.FullName == reader.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }

                                    try // try to find type in entry assembly
                                    {
                                        var assembly = Assembly.GetAssembly(typeof(ExtendedVplControl));
                                        type = assembly.GetTypes().First(t => t.FullName == reader.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }

                                    try // try to find type in ExternalNodeTypes
                                    {
                                        type = MainVplControl.ExternalNodeTypes.ToArray().First(t => t.FullName == reader.Name);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }

                                if (type != null)
                                {
                                    if (type.IsSubclassOf(typeof(Node)))
                                    {
                                        var node = (Node) Activator.CreateInstance(type, this.MainVplControl);
                                        node.DeserializeNetwork(reader);
                                        MainVplControl.NodeCollection.Add(node);
                                    }
                                    // TODO Check if the Start or End Indes is -1. If so it is an extendedPort which is outside the Control 
                                    else if (type == typeof(Connector))
                                    {
                                        Node startNode = null;
                                        Node endNode = null;

                                        var startNodeGuid = reader.GetAttribute("StartNode");
                                        if (startNodeGuid != null)
                                        {
                                            startNode =
                                                MainVplControl.NodeCollection.FirstOrDefault(
                                                    node => node.Guid == new Guid(startNodeGuid));
                                        }

                                        var endNodeGuid = reader.GetAttribute("EndNode");
                                        if (endNodeGuid != null)
                                        {
                                            endNode =
                                               MainVplControl.NodeCollection.FirstOrDefault(
                                                   node => node.Guid == new Guid(endNodeGuid));
                                        }
                                           
                                        var value = reader.GetAttribute("StartIndex");
                                        var startIndex = Convert.ToInt32(value);

                                        value = reader.GetAttribute("EndIndex");
                                        var endIndex = Convert.ToInt32(value);

                                        if (startNode != null && endNode != null && startIndex >= 0 && endIndex >= 0)
                                        {
                                            var startPort = startNode.OutputPorts[startIndex];
                                            var endPort = endNode.InputPorts[endIndex];

                                            if (startPort != null && endPort != null)
                                            {
                                                var connector = new Connector(MainVplControl, startPort, endPort);
                                                MainVplControl.ConnectorCollection.Add(connector);
                                            }
                                        }

                                        // ExtendedPort Connected which has no parentnode and no index ... 
                                        // Start Node is the 
                                        if (startIndex < 0)
                                        {
                                            foreach (var port in InputPortArea.PortControl.Children)
                                            {
                                                var extendedPort = port as ExtendedPort;
                                                if (Equals(extendedPort.ParentNode.Guid.ToString(), startNodeGuid))
                                                {
                                                    var newConnector = new Connector(MainVplControl, extendedPort, endNode.InputPorts[endIndex]);
                                                    MainVplControl.ConnectorCollection.Add(newConnector);
                                                }
                                            }
                                        }

                                        // ExtendedPort Connected ... 
                                        if (endIndex < 0)
                                        {
                                            foreach (var port in OutputPortArea.PortControl.Children)
                                            {
                                                var extendedPort = port as ExtendedPort;
                                                if (Equals(extendedPort.ParentNode.Guid.ToString(), endNodeGuid))
                                                {
                                                    var newConnector = new Connector(MainVplControl, startNode.OutputPorts[startIndex], extendedPort);
                                                    MainVplControl.ConnectorCollection.Add(newConnector);
                                                }
                                            }
                                        }

                                    }
                                    else if (type.IsSubclassOf(typeof(ExtendedPort)))
                                    {
                                        if (type.IsSubclassOf(typeof(ExtendedPort)))
                                        {
                                            var port = (ExtendedPort)Activator.CreateInstance(type, type.Name, PortTypes.Output,  typeof(int), MainVplControl);
                                            port.DeserializeNetwork(reader);
                                            if (port.PortType == PortTypes.Input)
                                            {
                                                OutputPortArea.AddPort(port);
                                            }
                                            else if (port.PortType == PortTypes.Output)
                                            {
                                                InputPortArea.AddPort(port);
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        case XmlNodeType.Text:

                            break;
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:

                            break;
                        case XmlNodeType.Comment:

                            break;
                        case XmlNodeType.EndElement:

                            break;
                    }
                }
            }
        }

        // Serialization ... 
        // private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        // {
        //     var path = @"C:\Users\Cornelius\Desktop\Test.vplxml";
        //     SerializeNetwork(path);
        //     Deserialize(path);
        // }

    }
}

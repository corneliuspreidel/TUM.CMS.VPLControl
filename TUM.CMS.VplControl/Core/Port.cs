using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using TUM.CMS.VplControl.Utilities;

namespace TUM.CMS.VplControl.Core
{
    public class Port : Control
    {
        private object data;
        private VplControl HostCanvas;

        public Port(string name, Node parent, PortTypes portType, Type type)
        {
            ParentNode = parent;
            HostCanvas = ParentNode.HostCanvas;
            DataType = type;
            PortType = portType;
            Name = name;

            if (portType == PortTypes.Input)
                Style = HostCanvas.FindResource("VplPortStyleLeft") as Style;
            else
                Style = HostCanvas.FindResource("VplPortStyleRight") as Style;

            MouseDown += Port_MouseDown;
            ParentNode.SizeChanged += ParentNode_SizeChanged;

            ParentNode.PropertyChanged += ParentNode_PropertyChanged;
            ConnectedConnectors = new List<Connector>();
            Origin = new BindingPoint(0, 0);
        }

        public Port(string name, PortTypes portType, Type type, VplControl hostCanvas)
        {
            DataType = type;
            PortType = portType;
            Name = name;

            HostCanvas = hostCanvas;

            if (portType == PortTypes.Input)
                Style = HostCanvas.FindResource("VplPortStyleLeft") as Style;
            else
                Style = HostCanvas.FindResource("VplPortStyleRight") as Style;

            MouseDown += Port_MouseDown;
            // ParentNode.SizeChanged += ParentNode_SizeChanged;

            // ParentNode.PropertyChanged += ParentNode_PropertyChanged;
            ConnectedConnectors = new List<Connector>();
            Origin = new BindingPoint(0,0);
        }

        public string Text
        {
            get
            {
                if (Data != null)
                    return Name + " : " + DataType.Name + " : " + Data;
                return Name + " : " + DataType.Name + " : null";
            }
        }

        public new string Name { get; set; }
        public Ellipse Geometry { get; set; }
        public Node ParentNode { get; set; }
        public PortTypes PortType { get; set; }
        public Type DataType { get; set; }

        public object Data
        {
            get { return data; }
            set
            {
                CalculateData(value);
            }
        }

        public bool MultipleConnectionsAllowed
        {
            get; set; 
            
        }

        public BindingPoint Origin { get; set; }
        public List<Connector> ConnectedConnectors { get; set; }

        private void ParentNode_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalcOrigin();
        }

        private void ParentNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CalcOrigin();
        }

        private void CalcOrigin()
        {
            Origin.X = TranslatePoint(new Point(Width/2, Height/2), HostCanvas).X;
            Origin.Y = TranslatePoint(new Point(Width/2, Height/2), HostCanvas).Y;
        }

        private void Port_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (HostCanvas.SplineMode)
            {
                case SplineModes.Nothing:
                    HostCanvas.TempStartPort = this;
                    HostCanvas.SplineMode = SplineModes.Second;
                    break;
                case SplineModes.Second:
                    if (
                        (
                            (
                                HostCanvas.TempStartPort.DataType.IsCastableTo(DataType) &&
                                HostCanvas.TypeSensitive && PortType == PortTypes.Output
                                ||
                                DataType.IsCastableTo(HostCanvas.TempStartPort.DataType) &&
                                HostCanvas.TypeSensitive && PortType == PortTypes.Input
                                ) // data types matching
                            ||
                            (!HostCanvas.TypeSensitive) // data types must not match
                            )
                        && PortType != HostCanvas.TempStartPort.PortType
                            // is not same port type --> input to output or output to input
                        && !Equals(ParentNode, HostCanvas.TempStartPort.ParentNode)) // is not same node
                    {
                        Connector connector;

                        if (PortType == PortTypes.Output)
                        {
                            if (HostCanvas.TempStartPort.ConnectedConnectors.Count > 0)
                            {
                                if (!HostCanvas.TempStartPort.MultipleConnectionsAllowed)
                                {
                                    foreach (var tempConnector in HostCanvas.TempStartPort.ConnectedConnectors)
                                        tempConnector.RemoveFromCanvas();

                                    HostCanvas.TempStartPort.ConnectedConnectors.Clear();
                                }
                            }

                            connector = new Connector(HostCanvas, this, HostCanvas.TempStartPort);
                        }
                        else
                        {
                            if (ConnectedConnectors.Count > 0)
                            {
                                if (!MultipleConnectionsAllowed)
                                {
                                    foreach (var tempConnector in ConnectedConnectors)
                                        tempConnector.RemoveFromCanvas();      

                                    ConnectedConnectors.Clear();
                                }
        
                            }

                            connector = new Connector(HostCanvas, HostCanvas.TempStartPort, this);
                        }

                        HostCanvas.ConnectorCollection.Add(connector);
                    }


                    HostCanvas.SplineMode = SplineModes.Nothing;
                    HostCanvas.ClearTempLine();
                    break;
            }

            e.Handled = true;
        }

        public void StartPort_DataChanged(object sender, EventArgs e)
        {
            CalculateData();
        }

        public event EventHandler DataChanged;

        public void OnDataChanged()
        {
            if (DataChanged != null)
                DataChanged(this, new EventArgs());
        }

        public void CalculateData(object value=null)
        {
            if (PortType == PortTypes.Input)
            {
                if (MultipleConnectionsAllowed && ConnectedConnectors.Count > 1)
                {
                    var listType = typeof (List<>).MakeGenericType(new Type[] {DataType});
                    IList list = (IList) Activator.CreateInstance(listType);

                    foreach (var conn in ConnectedConnectors)
                    {
                         list.Add(conn.StartPort.Data);
                    }

                    data = list;
                }
                else if (ConnectedConnectors.Count > 0)
                {
                    data = ConnectedConnectors[0].StartPort.Data;
                }
                else
                {
                    data = null;
                }
            }
            else
            {
                data = value;
            }

            OnDataChanged();
        }
    }

    public enum PortTypes
    {
        Input,
        Output
    }
}
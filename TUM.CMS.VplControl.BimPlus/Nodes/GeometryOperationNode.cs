using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Xml;
using BimPlus.IntegrationFramework.Contract.Model;
using BimPlus.IntegrationFramework.GeometryHelper;
using BimPlus.Sdk.Data.Geometry;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class GeometryOperationNode : Node
    {
        // DataController
        private readonly DataController _controller;
        private readonly ComboBox _typeComboBox;

        public GeometryOperationNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Input
            AddInputPortToNode("InputElements_1", typeof(List<GenericElement>));
            // Input
            AddInputPortToNode("InputElements_2", typeof(List<GenericElement>));

            // Output
            AddOutputPortToNode("ResultElements", typeof(object));
        }

        public override void Calculate()
        {
            if (InputPorts[0].Data == null || InputPorts[1].Data == null)
                return;

            var list_1 = InputPorts[0].Data as List<GenericElement>;
            var list_2 = InputPorts[1].Data as List<GenericElement>;

            var geomlist_1 = new List<DbGeometry>();
            var geomlist_2 = new List<DbGeometry>();

            foreach (var elem in list_1)
            {
                geomlist_1.AddRange(_controller.IntBase.APICore.GetElementGeometryAsDbGeometry(elem.Id));
            }

            foreach (var elem in list_2)
            {
                geomlist_2.AddRange(_controller.IntBase.APICore.GetElementGeometryAsDbGeometry(elem.Id));
            }

            // var res = GeometryKernel.ElementsTouch(geomlist_1, geomlist_2);

            // Start with multiThreading
            var res = GeometryKernel.ElementsTouch(geomlist_1, geomlist_2);

            OutputPorts[0].Data = res;
        }

        public override Node Clone()
        {
            return new GeometryOperationNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            /*
            var controlElement = ControlElements[0] as ComboBox;
            if (controlElement == null) return;
           
            xmlWriter.WriteStartAttribute("TypeName");
            xmlWriter.WriteValue(_typeComboBox.Name);
            xmlWriter.WriteEndAttribute();
            */
        }
    }
}
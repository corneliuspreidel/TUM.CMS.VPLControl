using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BimPlus.Sdk.Data.DbCore;
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Linq;
using QL4BIMspatial;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.Relations.Data;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class OperationalSpaceNode : Node
    {
        // DataController
        private readonly DataController _controller;
        private object result;

        public OperationalSpaceNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            // Init the QL4BIM framework
            // Commented by CP on 24.04
            //ql4Spatial.InitializeSettings();

            // Input
            AddInputPortToNode("InputElements_1", typeof(List<DtObject>));
            // Input
            AddInputPortToNode("InputElements_2", typeof(List<DtObject>));
            // Output
            AddOutputPortToNode("Relation", typeof(Relation));
        }

        private void ButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (InputPorts[0].Data == null || InputPorts[1].Data == null)
                return;

            
            OutputPorts[0].Data = result;
        }

        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return new GeometryOperationNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
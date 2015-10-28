﻿using System.Collections.Generic;
using System.Windows.Controls;
using BimPlus.IntegrationFramework.Contract.Model;
using TUM.CMS.VplControl.Nodes;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class RelationCreatorNode : Node
    {
        private DataController _controller { get; set; }
        private ComboBox combo { get; set; }

        public RelationCreatorNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            // Call the Singleton Class to get the actual loaded elements -> Connection to the DataModel
            _controller = DataController.Instance;

            DataContext = this;

            combo = new ComboBox();

            combo.Items.Add(new ComboBoxItem { Content = "touches"});
            combo.Items.Add(new ComboBoxItem { Content = "overlaps" });
            combo.Items.Add(new ComboBoxItem { Content = "is inside" });
            combo.Items.Add(new ComboBoxItem { Content = "usw." });
    
            combo.SelectedIndex = 0;
            AddControlToNode(combo);

            AddInputPortToNode("Input_1", typeof(List<GenericElement>));
            AddInputPortToNode("Input_2", typeof(List<GenericElement>));
            AddOutputPortToNode("Output", typeof (object));
        }

        public override void Calculate()
        {
            // Check null expression
            if (InputPorts[0].Data == null || InputPorts[1].Data == null) return;

            var res = new List<int>();

            // Get the different mechanisms 
            if (combo.SelectedItem.ToString() == "touches")
            {
                var inputElements_1 = InputPorts[0].Data as List<GenericElement>;
                var inputElements_2 = InputPorts[0].Data as List<GenericElement>;

                foreach (var item_1 in inputElements_1)
                {
                    foreach (var item_2 in inputElements_2)
                    {
                        // BBox check 

                        // if BBox check == true -> Triangle check ...
                    
                    }
                }
            }

            /*
            foreach (var item1 in InputPorts[0].Data as List<GenericElement>)
            {
                var poly1 = new CElementPolyeder();
                if (item1.Polyeder != null)
                    poly1.AddBasePolyeder(item1.Polyeder);
                var item2S = InputPorts[1].Data as List<GenericElement>;
                if (item2S == null) continue;
                foreach (var item2 in item2S)
                {
                    var poly2 = new CElementPolyeder();
                    if (item2.Polyeder != null)
                        poly2.AddBasePolyeder(item2.Polyeder);

                    // Geometric check ... 
                    res.Add(Nemetschek.Functions.Geometry_Functions.SchneidenSich(poly1, poly2));
                    var schnitt = Nemetschek.Functions.Geometry_Functions.Schneide2BelKoerper(poly1, poly2);
                }
            }
            */

            OutputPorts[0].Data = res;
        }

        public override Node Clone()
        {
            return new RelationCreatorNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
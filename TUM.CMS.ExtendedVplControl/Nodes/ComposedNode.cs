using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Nodes
{
    public class ComposedNode: Node
    {
        private TextBlock textBlock;
        public Dictionary<Guid, Port> portMap;

        // FilePath to the serialized Content of the Control 
        public String filePath;

        /// <summary>
        /// This Node may be composed of other Elements (Base Operators and Elements)
        /// </summary>
        /// <param name="hostCanvas"></param>
        public ComposedNode(VplControl.Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // Define the file path 
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                              @"\Nemetschek\bim+\ComposedNodes\" + Guid + ".vpxml";

            if (!File.Exists(filePath))
                File.Create(filePath);

            // Label
            var lbl = new Label()
            {
                Content = "Composition Node",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            AddControlToNode(lbl);
            // TextBox
            var textBox = new TextBox()
            {
                MinWidth = 150,
                Text = "Enter Method Name here ..."
            };
            textBox.TextChanged += TextBoxOnTextChanged;
            AddControlToNode(textBox);

            portMap = new Dictionary<Guid, Port>();
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var txtBox = sender as TextBox;
            if (txtBox == null)
                return;
            Name = txtBox.Text; 
        }

        public override void Calculate()
        {
            // 
        }

        public override Node Clone()
        {
            return null; 
        }

        public void AddOutputPortToNode(String name, Type dataType)
        {
            base.AddOutputPortToNode(name, dataType);
            portMap.Add(Guid.NewGuid(), OutputPorts[OutputPorts.Count - 1]);
        }

        public void AddInputPortToNode(String name, Type dataType)
        {
            base.AddInputPortToNode(name, dataType);
            portMap.Add(Guid.NewGuid(), InputPorts[InputPorts.Count - 1]);
        }
    }
}

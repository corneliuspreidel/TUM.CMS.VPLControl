using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Microsoft.Win32;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Ports.Input
{
    public class FilePathPort: ExtendedPort
    {
        private TextBlock textBlock; 

        public FilePathPort(string name, PortTypes portType, Type type, VplControl.Core.VplControl hostCanvas) : base(name, portType, type, hostCanvas)
        {
            // Set the DataType
            DataType = typeof(string);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            textBlock = new TextBlock() { MinWidth = 120, MaxWidth = 300, IsHitTestVisible = false };

            if (Data != null)
                textBlock.Text = (string)Data;

            var button = new Button
            {
                Content = "Search",
                Width = 50,
                Margin = new Thickness(5)
            };

            button.Click += button_Click;
            
            grid.Children.Add(textBlock);
            grid.Children.Add(button);

            AddPopupContent(grid);
        }

        void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                //Filter = "vplXML (.vplxml)|*.vplxml"
            };


            if (openFileDialog.ShowDialog() == true)
            {
                textBlock.Text = openFileDialog.FileName;
                Data = textBlock.Text;
            }
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartAttribute("Data");
            xmlWriter.WriteValue(Data);
            xmlWriter.WriteEndAttribute();
            base.SerializeNetwork(xmlWriter);
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            base.DeserializeNetwork(xmlReader);
            var value = xmlReader.GetAttribute("Data");
            if (value != null)
                Data = value;
        }
    }
}

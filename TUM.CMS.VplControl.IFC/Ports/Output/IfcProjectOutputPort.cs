using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.Core;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Ports.Output
{
    public class IfcProjectOutputPort: ExtendedPort
    {
        public IfcProjectOutputPort(string name, PortTypes portType, Type type, Core.VplControl hostCanvas, Guid id = new Guid()) 
            : base(name, portType, type, hostCanvas, id)
        {
            var modelInfo = Data as IfcModel;
            if (modelInfo == null)
                return;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "IfcFile |*.ifc",
                Title = "Save an IFC File"
            };
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                modelInfo.xModel.SaveAs(saveFileDialog.FileName);
                modelInfo.xModel.Close();
                if (File.Exists(saveFileDialog.FileName))
                {
                    MessageBox.Show("File saved", "My Application", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show("There was an Error \n Please Try again", "My Application", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Please Connect a Model", "My Application", MessageBoxButton.OK);
            }

        }
    }
}

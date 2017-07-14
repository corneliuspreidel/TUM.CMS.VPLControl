using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TUM.CMS.VplControl.IFC.Utilities;

namespace TUM.CMS.VplControl.IFC.Controls
{
    /// <summary>
    /// Interaction logic for PedSimNodeControl1.xaml
    /// </summary>
    public partial class PedSimNodeControl : UserControl
    {
        public PedSimViewer viewer;

        public PedSimNodeControl()
        {
            InitializeComponent();
            viewer = new PedSimViewer();
            viewerContentControl.Content = viewer;
        }

        private void ShowGraphCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            var chkBox = sender as CheckBox;
            if (chkBox.IsChecked == true)
            {
                string configFilePath = "\"D:\\dev\\TUM.CMS.VPLControl\\momenTUMv2\\bim2simGraph.xml\"";

                string momentTUMFilePath = "\"D:\\dev\\TUM.CMS.VPLControl\\momenTUMv2\\momenTUMv2.jar\"";
                string strCmdText = "java -jar " + momentTUMFilePath + "  --config " + configFilePath;

                // Process.Start("cmd.exe", strCmdText);

                viewer.ShowGraphModel(@"D:\dev\TUM.CMS.VPLControl\momenTUMv2\OUTPUT.xml");
            }
            else
            {
                viewer.RemoveGraphModel();
            }

            
        }
    }
}

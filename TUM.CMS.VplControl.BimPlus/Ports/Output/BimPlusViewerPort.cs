using System;
using System.IO;
using System.Windows.Controls;
using BimPlus.Client.WebControls.WPF;
using BimPlus.Sdk.Data.TenantDto;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.BimPlus.Utilities;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Ports.Output
{
    public class BimPlusViewerPort : ExtendedPort
    {
        private StreamWriter _streamWriter;
        private DataController _dataController;
        private readonly WebViewer _webViewer;

        private Grid ViewerGrid; 

        public BimPlusViewerPort(string name, PortTypes portType, Type type, Core.VplControl hostCanvas)
            : base(name, portType, type, hostCanvas)
        {
            _dataController = DataController.Instance;

            if (_dataController.IntBase == null)
                return;

            _webViewer = new WebViewer(_dataController.IntBase);
            ViewerGrid = new Grid()
            {
                Height = 600,
                Width = 400
            };
            ViewerGrid.Children.Add(_webViewer);

            SetAllowTransparencyForPopup(false);
            AddPopupContent(ViewerGrid);

            DataChanged += OnDataChanged;
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            if (Data == null)
                return;
            if (Data.GetType() == typeof(DtoShortProject))
            {
                var dtoProject = Data as DtoShortProject;
                if (dtoProject != null) _webViewer.NavigateToControl(dtoProject.Id);
            }
        }
    }
}

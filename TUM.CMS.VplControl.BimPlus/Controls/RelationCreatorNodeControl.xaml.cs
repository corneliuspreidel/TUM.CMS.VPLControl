using System.Collections.ObjectModel;
using System.Windows;

namespace TUM.CMS.VplControl.BimPlus.Controls
{
    /// <summary>
    ///     Interaction logic for RelationNodeControl.xaml
    /// </summary>
    public partial class RelationCreatorNodeControl
    {

        private ObservableCollection<object> leftItems;
        private ObservableCollection<object> rightItems;

        public RelationCreatorNodeControl()
        {
            InitializeComponent();
            // Connection
            DataContext = this;
            // Init
            leftItems = new ObservableCollection<object>();
            rightItems = new ObservableCollection<object>();
        }
    }
}
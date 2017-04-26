using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Controls
{
    /// <summary>
    /// Interaction logic for PortSelection.xaml
    /// </summary>
    public partial class PortSelection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Type> _portTypes;

        public PortSelection()
        {
            InitializeComponent();
            DataContext = this;

            // Init members
            _portTypes = new ObservableCollection<Type>();

            // SearchTextBox.Focus();
        }

        #region PropertyChanged

        public ObservableCollection<Type> PortTypes
        {
            get { return _portTypes; }
            set
            {
                _portTypes = value;
                OnPropertyChanged("PortTypes");
            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private void PortSelection_OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }
    }
}

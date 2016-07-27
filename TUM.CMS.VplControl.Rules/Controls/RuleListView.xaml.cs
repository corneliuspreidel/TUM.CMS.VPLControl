using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TUM.CMS.VplControl.Rules.Controls
{
    /// <summary>
    /// Interaction logic for RuleListView.xaml
    /// </summary>
    public partial class RuleListView
    {
        public List<Type> resultingTypes { get; set; }

        public RuleListView(IEnumerable<Type> types)
        {
            InitializeComponent();
            resultingTypes = new List<Type>();
            listView1.ItemsSource = types;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                resultingTypes.Remove(item as Type);
            }

            foreach (var item in e.AddedItems)
            {
                resultingTypes.Add(item as Type);
            }
        }
    }
}

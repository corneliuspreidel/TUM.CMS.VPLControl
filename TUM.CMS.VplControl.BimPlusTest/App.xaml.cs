using System;
using System.Windows;

namespace TUM.CMS.VplControl.BimPlusTest
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            // Add the GenericTheme
            var rDictionary = new ResourceDictionary { Source = new Uri("/TUM.CMS.VplControl;component/Themes/Generic.xaml", UriKind.Relative) };
            Current.Resources.MergedDictionaries.Add(rDictionary);
        }
    }
}
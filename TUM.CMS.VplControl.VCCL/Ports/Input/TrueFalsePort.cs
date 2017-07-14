using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using TUM.CMS.ExtendedVplControl.Ports;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.VCCL.Ports.Input
{
    public class TrueFalsePort : ExtendedPort
    {
        private readonly ToggleButton toggleButton;

        public TrueFalsePort(string name, PortTypes portType, Type type, Core.VplControl hostCanvas)
            : base(name, portType, type, hostCanvas)
        {
            toggleButton = new ToggleButton
            {
                Width = 80,
                Margin = new Thickness(5)
            };

            toggleButton.Checked += toggleButton_Checked;
            toggleButton.Unchecked += toggleButton_Unchecked;

            toggleButton.IsChecked = true;

            AddPopupContent(toggleButton);
        }

        void toggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            toggleButton.Content = "False";
            Data = toggleButton.IsChecked;
        }

        void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            toggleButton.Content = "True";
            Data = toggleButton.IsChecked;
        }
    }
}

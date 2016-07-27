
using System;
using System.Windows;

namespace TUM.CMS.VplControl.Rules.Data
{
    public class Customer
    {
        public string Name { get; private set; }
        public bool IsPreferred { get; set; }

        public Customer(string name)
        {
            Name = name;
        }

        public void NotifyAboutDiscount()
        {
            MessageBox.Show("Customer " + Name + " was notified about a discount");
        }
    }
}
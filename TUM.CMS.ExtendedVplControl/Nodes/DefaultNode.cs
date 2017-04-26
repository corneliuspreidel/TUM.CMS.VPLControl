using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.ExtendedVplControl.Nodes
{
    public class DefaultNode: Node
    {
        /// <summary>
        /// This Node may be composed of other Elements (Base Operators and Elements)
        /// </summary>
        /// <param name="hostCanvas"></param>
        public DefaultNode(VplControl.Core.VplControl hostCanvas) : base(hostCanvas)
        {
        }

        public override void Calculate()
        {
            // 
        }

        public override Node Clone()
        {
            return null; 
        }
    }
}

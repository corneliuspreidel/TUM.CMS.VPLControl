﻿using System;
using System.Windows.Controls;
using TUM.CMS.VplControl.BimPlus.BaseNodes;
using TUM.CMS.VplControl.BimPlus.Controls;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.BimPlus.Nodes
{
    public class WebBrowserNode : UtilityNode
    {
        // DataController
        // private DataController _controller;

        public WebBrowserNode(Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            DataContext = this;

            /*
            Frame frame = new Frame();
            frame.Navigate(new Uri("http://bbc.co.uk"));
            */

            var control = new WebBrowserControl();

            control.WebBrowser.Navigate(new Uri("http://portal-stage.bimplus.net/?embedded=1"));

            var pr = new ContentPresenter
            {
                Content = control,
                Width = 1000,
                Height = 1000
            };
            AddControlToNode(pr);
        }

        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return new WebBrowserNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
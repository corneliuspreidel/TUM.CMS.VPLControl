﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TUM.CMS.VplControl.Nodes;
using TUM.CMS.VplControl.Utilities;

namespace TUM.CMS.VplControl.Core
{
    internal class SelectionNode : Node
    {

        List<Type> typeList = new List<Type>();
        ListBox listBox = new ListBox();
        private SearchTextBox searchTextBox;

        public SelectionNode(VplControl hostCanvas) : base(hostCanvas)
        {
            searchTextBox = new SearchTextBox();
            searchTextBox.OnSearch += searchTextBox_OnSearch;

            List<Type> tempTypeList = new List<Type>();
            AddControlToNode(searchTextBox);

            listBox.DisplayMemberPath = "Name";
            listBox.MaxHeight = 140;

            AddControlToNode(listBox);


            switch (hostCanvas.NodeTypeMode)
            {
                case NodeTypeModes.OnlyInternalTypes:
                    tempTypeList.AddRange(
                        ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Nodes")
                            .ToList());
                    break;
                case NodeTypeModes.OnlyExternalTypes:
                    tempTypeList.AddRange(hostCanvas.ExternalNodeTypes);
                    break;
                case NodeTypeModes.All:
                    tempTypeList.AddRange(
                        ClassUtility.GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TUM.CMS.VplControl.Nodes")
                            .ToList());
                    tempTypeList.AddRange(hostCanvas.ExternalNodeTypes);
                    break;
            }

            tempTypeList = tempTypeList.OrderBy(x => x.Name).ToList();



            foreach (var type in tempTypeList.Where(type => !type.IsAbstract))
            {
                typeList.Add(type);
            }

            listBox.ItemsSource = typeList;
            searchTextBox.PreviewKeyDown += searchTextBox_KeyDown;
            listBox.PreviewMouseLeftButtonUp += listBox_PreviewMouseLeftButtonUp;
            
            listBox.SelectionMode= SelectionMode.Single;


            Border.MouseLeave += SelectionNode_MouseLeave;
            MouseEnter += SelectionNode_MouseEnter;
        }

        void listBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CreateNode();
        }



        void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            SearchTextBox textBox=sender as SearchTextBox;
           
            if (e.Key == Key.Enter && listBox.Items.Count>0)
                if ((textBox != null && textBox.Text != "") ||
                    (listBox.SelectedIndex > -1 && listBox.SelectedIndex < listBox.Items.Count+1)
                    )
                    CreateNode();
                else
                {
                    Dispose();
                    HostCanvas.Children.Remove(Border);
                }
            else switch (e.Key)
            {
                case Key.Down:
                    if (listBox.SelectedIndex < listBox.Items.Count)
                    {
                        listBox.SelectedIndex += 1;
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (listBox.SelectedIndex > 0) listBox.SelectedIndex -= 1;
                    e.Handled = true;
                    break;
            } 
        }

        void SelectionNode_MouseEnter(object sender, MouseEventArgs e)
        {
            ControlElements[0].Focus();
        }

        private void SelectionNode_MouseLeave(object sender, MouseEventArgs e)
        {
            Dispose();
            HostCanvas.Children.Remove(Border);
        }


        void searchTextBox_OnSearch(object sender, RoutedEventArgs e)
        {
            SearchEventArgs searchArgs = e as SearchEventArgs;

            if (searchArgs == null) return;


            if (searchArgs.Keyword == "")
            {
                listBox.ItemsSource = typeList;
                listBox.SelectedIndex = -1;
            }
            else
            {
                listBox.ItemsSource = typeList
                    .Where(x=> x.Name.StartsWith(searchArgs.Keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                listBox.SelectedIndex = 0;

            }
        }


        private void CreateNode()
        {
            var selectedType = listBox.SelectedItem as Type;
            if (selectedType == null) return;

            var node = (Node)Activator.CreateInstance(selectedType, HostCanvas);

            node.Left = Left;
            node.Top = Top;

            HostCanvas.Children.Remove(Border);
            HostCanvas.NodeCollection.Add(node);
        }


        public override void Calculate()
        {
        }

        public override Node Clone()
        {
            return (Node) MemberwiseClone();
        }
    }
}
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TUM.CMS.ExtendedVplControl.Utilities
{
    public class WatchPortControl: UserControl, INotifyPropertyChanged
    {
        private ScrollViewer scrollViewer;
        private TextBlock textBlock;
        private object _data;

        public WatchPortControl()
        {
            textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                FontSize = 14,
                Padding = new Thickness(5),
                IsHitTestVisible = false
            };

            scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MinWidth = 120,
                MinHeight = 20,
                CanContentScroll = true,
                Content = textBlock
            };

            AddChild(scrollViewer);
        }

        private void OnDataChanged(object sender, EventArgs eventArgs)
        {
            if (Data == null)
                textBlock.Text = "null";
            else
            {
                Type t;

                var type = Data as Type;

                if (type != null)
                {
                    t = type;

                    if (t.IsEnum)
                    {
                        textBlock.Text = "Enum " + t.Name + " {";

                        var counter = 0;
                        foreach (var o in Enum.GetValues(t))
                        {
                            textBlock.Text += o.ToString();

                            if (counter < Enum.GetValues(t).Length - 1)
                                textBlock.Text += ",";

                            counter++;
                        }
                        textBlock.Text += "}";
                    }
                    else
                        textBlock.Text = t.Name + " : Type";
                }
                else
                {
                    t = Data.GetType();

                    if (t.IsGenericType)
                    {
                        var collection = Data as ICollection;
                        if (collection == null) return;
                        var obj = collection;

                        textBlock.Text = CollectionToString(obj, 1);
                    }
                    else
                        textBlock.Text = Data + " : " + t.Name;
                }
            }
        }

        private string CollectionToString(ICollection coll, int depth)
        {
            var tempLine = "";

            for (var i = 0; i < depth - 1; i++)
                tempLine += "  ";

            tempLine = "List" + Environment.NewLine;
            var counter = 0;

            foreach (var item in coll)
            {
                for (var i = 0; i < depth; i++)
                    tempLine += "  ";

                tempLine += "[" + counter + "] ";

                if (item == null)
                {
                    tempLine += "null";

                    if (depth != 1 || counter != coll.Count - 1)
                        tempLine += Environment.NewLine;
                }
                else
                {
                    if (item.GetType().IsGenericType)
                    {
                        var collection = item as ICollection;
                        if (collection == null) return "";
                        var obj = collection;

                        tempLine += CollectionToString(obj, depth + 1);
                    }
                    else
                    {
                        tempLine += item + " : " + item.GetType().Name;

                        if (depth != 1 || counter != coll.Count - 1)
                            tempLine += Environment.NewLine;
                    }
                }

                counter++;
            }
            return tempLine;
        }

        #region PropertyChangedStuff

        public object Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
                OnDataChanged(null, null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}

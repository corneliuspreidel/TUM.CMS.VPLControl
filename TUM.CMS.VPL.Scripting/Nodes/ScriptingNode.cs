using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using TUM.CMS.VplControl.Nodes;
using TUM.CMS.VPL.Scripting.CSharp;
using TUM.CMS.VPL.Scripting.Python;

namespace TUM.CMS.VPL.Scripting.Nodes
{
    public class ScriptingNode : Node
    {
        private ScriptingControl scriptingControl;
        private Lazy<CSharpScriptCompiler> mScriptCompiler;
        private string scriptContent;

        public ScriptingNode(VplControl.Core.VplControl hostCanvas)
            : base(hostCanvas)
        {
            scriptingControl = new ScriptingControl();

            scriptingControl.HighlightingComboBox.SelectionChanged += HighlightingComboBoxOnSelectionChanged;

            // Tighten Button
            var tightenButton = new Button();
            tightenButton.Click += TightenButtonOnClick;
            tightenButton.Content = "x";
            tightenButton.HorizontalAlignment = HorizontalAlignment.Right;
            tightenButton.VerticalAlignment = VerticalAlignment.Top;
            // AddControlToNode(tightenButton);

            // Create new script File
            scriptingControl.CurrentFile = new CSharpScriptFile2();

            IsResizeable = true;

            scriptingControl.StartCSharpCompilingEventHandler += StartCSharpCompilation;
            scriptingControl.StartPythonCompilingEventHandler += StartPythonCompilation;

            AddControlToNode(scriptingControl);

            AddInputPortToNode("Input", typeof (object));

            AddOutputPortToNode("Output", typeof (object));
        }

        private void TightenButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            switch (scriptingControl.Visibility)
            {
                case Visibility.Visible:
                    scriptingControl.Visibility = Visibility.Collapsed;
                    break;
                case Visibility.Collapsed:
                    scriptingControl.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void HighlightingComboBoxOnSelectionChanged(object sender,
            SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (scriptingControl.HighlightingComboBox.SelectedItem == null)
                return;
            switch (scriptingControl.HighlightingComboBox.SelectedItem.ToString())
            {
                case "C#":
                    scriptingControl.CurrentFile = new CSharpScriptFile2();
                    break;
                case "Python":
                    scriptingControl.CurrentFile = new PythonScriptFile();
                    break;
            }
            scriptingControl.TextEditor.Text = scriptingControl.CurrentFile.ScriptContent;
        }

        public void StartCSharpCompilation(object sender, EventArgs eventArgs)
        {
            if (TopComment.Visibility == Visibility.Visible)
            {
                TopComment.Visibility = Visibility.Hidden;
                TopComment.HostNode_PropertyChanged(null, null);
            }

            scriptingControl.CurrentFile.ScriptContent = scriptingControl.TextEditor.Text;

            // Define the Input for the Script
            try
            {
                mScriptCompiler = new Lazy<CSharpScriptCompiler>();


                //Compile and execute current script
                var result = mScriptCompiler.Value.Compile(scriptingControl.CurrentFile as CSharpScriptFile);


                if (result.GetType() == typeof (CompilerErrorCollection))
                {
                    var compilerErrors = new StringBuilder();
                    var compilerErrorCollection = result as CompilerErrorCollection;
                    if (compilerErrorCollection == null)
                    {
                        throw new InvalidOperationException(
                            "Unable to compile scripts: " +
                            compilerErrors);
                    }
                    foreach (var actError in compilerErrorCollection)
                        compilerErrors.Append("" + actError + Environment.NewLine);
                    TopComment.Text = compilerErrors.ToString();
                    TopComment.Visibility = Visibility.Visible;
                    TopComment.HostNode_PropertyChanged(null, null);
                    throw new InvalidOperationException(
                        "Unable to compile scripts: " +
                        compilerErrors);
                }


                var parameter = mScriptCompiler.Value.ScriptMethod.GetParameters();

                var counter = 0;
                foreach (var para in parameter)
                {
                    if (counter < InputPorts.Count)
                    {
                        if (para.ParameterType.IsGenericType)
                        {
                            InputPorts[counter].MultipleConnectionsAllowed = true;
                            InputPorts[counter].DataType = para.ParameterType.GetGenericArguments()[0];
                        }
                        else
                        {
                            InputPorts[counter].DataType = para.ParameterType;
                        }

                        InputPorts[counter].Name = para.Name;
                    }
                    else
                    {
                        if (para.ParameterType.IsGenericType)
                            AddInputPortToNode(para.Name, para.ParameterType.GetGenericArguments()[0], true);
                        else
                            AddInputPortToNode(para.Name, para.ParameterType);
                    }


                    counter++;
                }

                while (counter < InputPorts.Count())
                    RemoveInputPortFromNode(InputPorts.Last());

                Calculate();

                scriptingControl.TextBlockError.Text = "";
                scriptingControl.TextBlockError.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                TopComment.Text = e.ToString() ;
                TopComment.Visibility = Visibility.Visible;
                TopComment.HostNode_PropertyChanged(null, null);

                scriptingControl.TextBlockError.Text = e.Message;
                if (e.InnerException != null)
                {
                    scriptingControl.TextBlockError.Text = scriptingControl.TextBlockError.Text
                                                           + "\n InnerException: \n" + e.InnerException.ToString();
                }
                scriptingControl.TextBlockError.Visibility = Visibility.Visible;

                Console.WriteLine(e.Message);
            }
        }

        public void StartPythonCompilation(object sender, EventArgs eventArgs)
        {
            if (TopComment.Visibility == Visibility.Visible)
            {
                TopComment.Visibility = Visibility.Hidden;
                TopComment.HostNode_PropertyChanged(null, null);
            }

            var file = new PythonScriptFile {ScriptContent = scriptingControl.TextEditor.Text};

            try
            {
                var compiler = new PythonScriptCompiler();

                var obj = compiler.CompileSourceAndExecute(file.ScriptContent, InputPorts[0].Data);
                if (obj != null)
                    OutputPorts[0].Data = obj;
            }
            catch (Exception e)
            {
                TopComment.Text = e.ToString();
                TopComment.Visibility = Visibility.Visible;
                TopComment.HostNode_PropertyChanged(null, null);
            }
        }

        public override void Calculate()
        {


            // Define the Input for the Script
            var parameters = InputPorts.Select(port => port.Data).ToList();

            switch (scriptingControl.HighlightingComboBox.SelectedItem.ToString())
            {
                case "C#":
                    OutputPorts[0].Data = mScriptCompiler.Value.Run(parameters.ToArray());
                    break;
                case "Python":
                    StartPythonCompilation(null, null);
                    break;
            }
        }

        public override void SerializeNetwork(XmlWriter xmlWriter)
        {
            base.SerializeNetwork(xmlWriter);

            scriptContent = scriptingControl.TextEditor.Text;

            xmlWriter.WriteStartAttribute("_mSkriptContent");
            xmlWriter.WriteValue(scriptContent);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("_referencedAssemblies");
            xmlWriter.WriteValue(scriptingControl.CurrentFile.ReferencedAssemblies);
            xmlWriter.WriteEndAttribute();

            xmlWriter.WriteStartAttribute("language");
            xmlWriter.WriteValue(scriptingControl.HighlightingComboBox.SelectedItem.ToString());
            xmlWriter.WriteEndAttribute();
        }

        public override void DeserializeNetwork(XmlReader xmlReader)
        {
            var language = xmlReader.GetAttribute("language");
            scriptingControl.HighlightingComboBox.SelectedItem = language;

            base.DeserializeNetwork(xmlReader);
            scriptingControl.TextEditor.Text = xmlReader.GetAttribute("_mSkriptContent");

            var list = xmlReader.GetAttribute("_referencedAssemblies");

            if (list != null)
            {
                var result = list.Split(' ').ToList();
                foreach (var item in result)
                {
                    if(scriptingControl.CurrentFile.ReferencedAssemblies.Contains(item) != true)
                        scriptingControl.CurrentFile.ReferencedAssemblies.Add(item);
                }
            }

            if (language == "C#")
                StartCSharpCompilation(null, null);
            else
                StartPythonCompilation(null, null);
        }

        public override Node Clone()
        {
            return new ScriptingNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }
    }
}
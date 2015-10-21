using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;

namespace TUM.CMS.VPL.Scripting
{
    /// <summary>
    ///     Interaction logic for AddAssemblyDialog.xaml
    /// </summary>
    public partial class AssemblyDialog
    {
        private readonly ScriptFile scriptFile;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddAssemblyDialog" /> class.
        /// </summary>
        public AssemblyDialog(ScriptFile file)
        {
            InitializeComponent();
            scriptFile = file;
            ReferencedAssembliedListBox.ItemsSource = scriptFile.ReferencedAssemblies;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
        }

        /// <summary>
        ///     Called when user wants to confirm.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnCmdOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Called when user wants to cancel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void OnCmdCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dia = new OpenFileDialog
            {
                Filter = @"dll files (*.dll)|*.dll|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (dia.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            // Load Referenced Assemblies
            var ass = Assembly.LoadFrom(dia.FileName);
            /*
            var refAssemblies = ass.GetReferencedAssemblies();
            foreach (var refAss in refAssemblies)
            {
                // scriptFile.ReferencedAssemblies.Add();
                Assembly.Load(refAss);
            }
            */
            // ReferencedAssembliedListBox.Items.Add(dia.FileName);
            if (!scriptFile.ReferencedAssemblies.Contains(dia.FileName))
                scriptFile.ReferencedAssemblies.Add(dia.FileName);
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var item = ReferencedAssembliedListBox.SelectedItems[0];

            if (scriptFile.ReferencedAssemblies.Contains(item.ToString()))
            {
                scriptFile.ReferencedAssemblies.Remove(item.ToString());
            }
                
        }

        private void AssemblyTextBoxAddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AssemblyTextBox.Text != "")
            {
                scriptFile.ReferencedAssemblies.Add(AssemblyTextBox.Text);
            }
        }

        private void LoadAllAssembliesFromFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var dia = new FolderBrowserDialog();

            if (dia.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            // Load Referenced Assemblies from folder
            var files = Directory.GetFiles(dia.SelectedPath);

            foreach (var item in files)
            {
                if(item.EndsWith(".dll"))
                {
                    scriptFile.ReferencedAssemblies.Add(item);
                }
            }
        }
    }
}
// ToolkitSelectWindow.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frosty.Controls;

namespace FrostyMeshImporter.Windows
{
    /// <summary>
    /// Interaction logic for ToolkitSelectWindow.xaml
    /// </summary>

    public partial class ToolkitSelectWindow : FrostyDockableWindow
    {
        private struct ToolkitSelect
        {
            public string toolkitName { get; }
            public toolkit toolkitID { get; }

            public ToolkitSelect(string toolkitName, toolkit toolkitID)
            {
                this.toolkitName = toolkitName;
                this.toolkitID = toolkitID;
            }
        }

        public toolkit selectedToolkit = toolkit.Default;

        public ToolkitSelectWindow(toolkit currentToolkit)
        {
            InitializeComponent();
            List<ToolkitSelect> comboBoxChoices = new List<ToolkitSelect>();
            comboBoxChoices.Add(new ToolkitSelect("Mesh Import (Default)", toolkit.Default));
            comboBoxChoices.Add(new ToolkitSelect("FrostMeshy Import", toolkit.FrostMeshyImport));
            selectionBox.ItemsSource = comboBoxChoices;
            // Set selected value to current toolkit
            foreach (ToolkitSelect cur in comboBoxChoices)
            {
                if(cur.toolkitID == currentToolkit)
                {
                    selectionBox.SelectedItem = cur;
                    break;
                }
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ToolkitSelect selected = (ToolkitSelect)selectionBox.SelectedItem;
            selectedToolkit = selected.toolkitID;
            this.DialogResult = new bool?(true);
            this.Close();
        }

        public toolkit GetToolkit()
        {
            return selectedToolkit;
        }

        private void FrostyDockableWindow_FrostyLoaded(object sender, EventArgs e)
        {

        }
    }
}

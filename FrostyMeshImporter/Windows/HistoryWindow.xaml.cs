// HistoryWindow.xaml.cs - FrostyMeshImporter
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
using Frosty.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using MeshSet = FrostyMeshImporter.Toolkits.MeshImport.ChunkResImporter.MeshSet;

namespace FrostyMeshImporter.Windows
{
    /// <summary>
    /// Interaction logic for HistoryWindow.xaml
    /// </summary>
    /// 
    public partial class HistoryWindow : FrostyDockableWindow 
    {
        public List<string> selectedItems;
        public bool revert;

        public HistoryWindow()
        {
            InitializeComponent();
            selectedItems = new List<string>();
        }

        internal void SetItems(List<MeshSet> items)
        {
            lbSelectAsset.ItemsSource = items;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
            this.Close();
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            revert = true;
            Run("re-import");
        }
        
        private void ReImportButton_Click(object sender, RoutedEventArgs e)
        {
            revert = false;
            Run("revert");
        }

        private void Run(string operation)
        {
            if (lbSelectAsset.SelectedItems.Count == 0)
            {
                FrostyMessageBox.Show($"Select at least one mesh to {operation}.", Program.IMPORTER_MESSAGE, MessageBoxButton.OK);
                return;
            }
            foreach (var selection in lbSelectAsset.SelectedItems)
            {
                selectedItems.Add(selection.ToString());
            }
            this.DialogResult = new bool?(true);
            this.Close();
        }

        private void FrostyDockableWindow_FrostyLoaded(object sender, EventArgs e)
        {
            
        }
    }
}

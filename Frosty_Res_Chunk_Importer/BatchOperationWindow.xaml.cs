// BatchOperationWindow.xaml.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Frosty.Controls;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Managers;
using FrostyEditor.Controls;
using FrostyEditor.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace FrostyResChunkImporter
{
    /// <summary>
    /// Interaction logic for BatchOperationWindow.xaml
    /// </summary>
    /// 
    public partial class BatchOperationWindow : FrostyDockableWindow 
    {
        public List<string> selectedItems;
        private string operation;


        public BatchOperationWindow(string operation)
        {
            InitializeComponent();
            this.operation = operation;
            this.Title = $"Batch {operation}";
            this.label.Content = $"Select one or more meshes to {operation}:";
            this.executeOrderButton.Content = $"{operation.Substring(0, 1).ToUpper()}{operation.Substring(1, operation.Length - 1)}";
            if(operation == "re-import")
            {
                this.revertCheckBox.IsEnabled = false;
            }
            List<ImportedAsset> items = ChunkResImporter.importedAssets.ToList<ImportedAsset>();
            lbSelectAsset.ItemsSource = items;
            selectedItems = new List<string>();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
            this.Close();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if(lbSelectAsset.SelectedItems.Count == 0)
            {
                FrostyMessageBox.Show($"Select at least one mesh to {operation}.", "Frost Res/Chunk Importer", MessageBoxButton.OK);
                return;
            }
            foreach(var selection in lbSelectAsset.SelectedItems)
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

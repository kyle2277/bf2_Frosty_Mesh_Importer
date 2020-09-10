using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Frosty.Controls;
using FrostyEditor.Controls;
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
    /// Interaction logic for RevertAsset.xaml
    /// </summary>
    /// 
    public partial class RevertAssetWindow : FrostyDockableWindow
    {
        public List<string> selectedItems;
        private string operation;

        public RevertAssetWindow(string operation)
        {
            InitializeComponent();
            this.operation = operation;
            this.Title = $"Batch {operation}";
            List<ImportedAsset> items = ChunkResImporter.importedAssets.ToList<ImportedAsset>();
            lbSelectAsset.ItemsSource = items;
            selectedItems = new List<string>();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = new bool?(false);
            this.Close();
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
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

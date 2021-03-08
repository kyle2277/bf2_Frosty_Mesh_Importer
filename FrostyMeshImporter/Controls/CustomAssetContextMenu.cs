using System;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostyMeshImporter.Controls
{
    class CustomAssetContextMenu
    {
        public int defaultCount;
        public int numAddedCommands;
        private FrostyDataExplorer dataExplorer;

        public CustomAssetContextMenu(FrostyDataExplorer dataExplorer)
        {
            this.dataExplorer = dataExplorer;
            defaultCount = dataExplorer.AssetContextMenu.Items.Count;
            numAddedCommands = 0;
        }

        public void ResetContextMenu()
        {
            var items = dataExplorer.AssetContextMenu.Items;
            while(numAddedCommands > 0)
            {
                items.RemoveAt(items.Count - 1);
                numAddedCommands -= 1;
            }
        }
        public void UpdateContextMenu(object sender, RoutedEventArgs e)
        {
            if(dataExplorer?.SelectedAsset is AssetEntry asset)
            {
                ResetContextMenu();
                if(asset.Type.Contains("MeshAsset"))
                {
                    MenuItem meshExport = new MenuItem();
                    meshExport.Click += Program.OnExportResourceFilesCommand;
                    meshExport.Header = "Export Mesh Files";
                    // Todo icon
                    dataExplorer.AssetContextMenu.Items.Add(meshExport);
                    numAddedCommands += 1;
                }
            }
        }
    }
}

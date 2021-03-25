// CustomAssetContextMenu.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostyMeshImporter.Program;

namespace FrostyMeshImporter.Controls
{

class CustomAssetContextMenu
    {
        public int defaultCount;
        public int numAddedCommands;
        private FrostyDataExplorer dataExplorer;
        // Context menu icons
        private static Image _editLabelIcon = new Image
        {
            Source = new BitmapImage(new Uri("/FrostyEditor;Component/Images/EditLabel.png", UriKind.Relative)),
            Opacity = 0.5
        };
        private static Image _exportIcon = new Image
        {
            Source = new BitmapImage(new Uri("/FrostyEditor;Component/Images/Export.png", UriKind.Relative)),
            Opacity = 0.5
        };

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
                    // Add res file export to context menu
                    MenuItem meshExport = new MenuItem();
                    meshExport.Click += OnExportResourceFilesCommand;
                    meshExport.Header = "Export Mesh Files";
                    meshExport.Icon = _exportIcon;
                    // Todo icon
                    dataExplorer.AssetContextMenu.Items.Add(meshExport);
                    numAddedCommands += 1;
                } else if(asset.Type.Equals("FsUITextDatabase"))
                {
                    // Add FrosTxt to context menu
                    MenuItem frosTxt = new MenuItem();
                    frosTxt.Click += OnFrosTxtCommand;
                    frosTxt.Header = "Open FrosTxt";
                    frosTxt.Icon = _editLabelIcon;
                    dataExplorer.AssetContextMenu.Items.Add(frosTxt);
                    numAddedCommands += 1;
                }
            }
        }
    }
}

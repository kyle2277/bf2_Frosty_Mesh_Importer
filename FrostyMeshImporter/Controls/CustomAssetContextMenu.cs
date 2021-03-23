﻿// CustomAssetContextMenu.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

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
using static FrostyMeshImporter.Program;

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
                    // Add res file export to context menu
                    MenuItem meshExport = new MenuItem();
                    meshExport.Click += OnExportResourceFilesCommand;
                    meshExport.Header = "Export Mesh Files";
                    // Todo icon
                    dataExplorer.AssetContextMenu.Items.Add(meshExport);
                    numAddedCommands += 1;
                } else if(asset.Type.Equals("FsUITextDatabase"))
                {
                    // Add FrosTxt to context menu
                    MenuItem frosTxt = new MenuItem();
                    frosTxt.Click += OnFrosTxtCommand;
                    frosTxt.Header = "Open FrosTxt";
                    dataExplorer.AssetContextMenu.Items.Add(frosTxt);
                    numAddedCommands += 1;
                }
            }
        }
    }
}

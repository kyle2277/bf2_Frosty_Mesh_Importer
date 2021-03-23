using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using Frosty.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Managers;
using Microsoft.Win32;
using System.IO;
using FrostySdk.Ebx;
using System.Web.SessionState;
using FrostyMeshImporter.Windows;
using MeshSet = FrostyMeshImporter.Toolkits.MeshImport.ChunkResImporter.MeshSet;
using FrostyMeshImporter.Toolkits.MeshImport;

namespace FrostyMeshImporter
{
    partial class Program
    {
        private static List<FrosTxtWindowObj> _localizationProfiles;
        private static AssetEntry _currentLocalization;
        private static FsUITextDatabase _currentTextDatabase;
        private static FrosTxtWindowObj _lastFrosTxtWindow;
        private static bool _openFrosTxt = false;
        private static Guid DEFAULT_ENGLISH_GUID = new Guid("d95231aa-79a3-64a3-ac02-6848d19021d5");

        private class FrosTxtWindowObj
        {
            public FrosTxtWindow window;
            public AssetEntry localizationAsset;
            public ChunkAssetEntry textChunk;

            public FrosTxtWindowObj(FrosTxtWindow window, AssetEntry localizationAsset, ChunkAssetEntry textChunk)
            {
                this.window = window;
                this.localizationAsset = localizationAsset;
                this.textChunk = textChunk;
            }
        }

        // Setup FrosTxt window to be opened.
        public static void OnFrosTxtCommand(object sender, RoutedEventArgs e)
        {
            CheckChunkResExplorerOpen();
            if(_localizationProfiles == null)
            {
                _localizationProfiles = new List<FrosTxtWindowObj>();
            }
            if(_mainWindowExplorer.SelectedAsset.Type.Equals("FsUITextDatabase"))
            {
                // Open FrosTxt window corresponding to current selected asset
                _currentLocalization = _mainWindowExplorer.SelectedAsset;
                _lastFrosTxtWindow = null;
            } else
            {
                // Open last FrosTxt window
                _currentLocalization = null;
                _mainWindowExplorer.SelectedAsset = _lastFrosTxtWindow.localizationAsset;
            }
            _mainWindowExplorer.DoubleClickSelectedAsset();
            _openFrosTxt = true;
        }

        // Opens FrosTxt window specified by lastFrosTxtWindow.
        // Pre-condition: lastFrosTxtWindow != null and contains an initialized FrosTxtWindow reference.
        private static void OpenFrosTxtWindow()
        {
            if(_currentLocalization != null && _lastFrosTxtWindow == null)
            {
                // Open FrosTxtWindow using the current open localization asset
                FsUITextDatabase data = (FsUITextDatabase)_currentAssetEditor.RootObject;
                // Check if FrosTxtWindow already in localizationProfiles
                // If so, set as last opened window and open
                // Else, create new window profile, add to localization profiles, and set as last opened window
                Guid chunkID = data.BinaryChunk;
                ChunkAssetEntry textChunk = App.AssetManager.GetChunkEntry(chunkID);
                Stream baseTextStream = App.AssetManager.GetChunk(textChunk);
                // create base file with chunk stream
            } else if(_currentLocalization == null && _lastFrosTxtWindow != null)
            {
                // Open FrosTxtWindow using the last opened window
                FsUITextDatabase data = (FsUITextDatabase)_currentAssetEditor.RootObject;
            } else  // No localization asset selected and no last FrosTxtWindow
            {
                // Open default english FrosTxtWindow
            }
        }

        public static bool CompareByAssetEntryName(AssetEntry a)
        {
            return a.Name == _searchTerm;
        }

        // Open FrosTxt reversion window.
        public static void OnRevertFrosTxtCommand(object sender, RoutedEventArgs e)
        {

        }
    }
}

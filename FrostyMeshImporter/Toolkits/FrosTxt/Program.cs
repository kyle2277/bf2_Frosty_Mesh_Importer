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
using FrosTxtCore;

namespace FrostyMeshImporter
{
    partial class Program
    {
        private static List<FrosTxtWindow> _localizationProfiles;
        private static EbxAssetEntry _currentLocalization;
        private static FsUITextDatabase _currentTextDatabase;
        private static FrosTxtWindow _lastFrosTxtWindow;
        private static bool _openFrosTxt = false;
        private static string DEFAULT_ENGLISH = "Localization/WSLocalization_English";
        private static string _tempChunk = ".\\FrosTxtTemp\\chunk.chunk";

        // Setup FrosTxt window to be opened.
        public static void OnFrosTxtCommand(object sender, RoutedEventArgs e)
        {
            CheckChunkResExplorerOpen();
            if(_localizationProfiles == null)
            {
                _localizationProfiles = new List<FrosTxtWindow>();
            }
            if(_mainWindowExplorer.SelectedAsset != null && 
                _mainWindowExplorer.SelectedAsset.Type.Equals("FsUITextDatabase"))
            {
                // Open FrosTxt window corresponding to current selected asset
                _currentLocalization = (EbxAssetEntry)_mainWindowExplorer.SelectedAsset;
                _lastFrosTxtWindow = null;
            } else if(_lastFrosTxtWindow != null)
            {
                // Open last FrosTxt window
                _currentLocalization = null;
                _mainWindowExplorer.SelectedAsset = _lastFrosTxtWindow.localizationAsset;
            } else  // lastFrosTxtWindow is null
            {
                EbxAssetEntry englishLocalization = App.AssetManager.GetEbxEntry(DEFAULT_ENGLISH);
                _mainWindowExplorer.SelectedAsset = englishLocalization;
                _currentLocalization = englishLocalization;
            }
            FsUITextDatabase testTab = _currentAssetEditor?.RootObject as FsUITextDatabase;
            if(testTab != null && _mainWindowExplorer.SelectedAsset != null && 
                testTab.Language.ToString().Split('_')[1] == _mainWindowExplorer.SelectedAsset.Name.Split('_')[1])
            {
                OpenFrosTxtWindow();
            } else
            {
                _mainWindowExplorer.DoubleClickSelectedAsset();
                _openFrosTxt = true;
            }
        }

        // Opens FrosTxt window specified by lastFrosTxtWindow.
        // Pre-condition: lastFrosTxtWindow != null and contains an initialized FrosTxtWindow reference.
        private static void OpenFrosTxtWindow()
        {
            Predicate<FrosTxtWindow> FrosTxtWindowPredicate = CompareByFrosTxtWindowLanguage;
            if (_currentLocalization != null)
            {
                // Open FrosTxtWindow using the current open localization asset
                _currentTextDatabase = (FsUITextDatabase)_currentAssetEditor.RootObject;
                // data.Name = WSLocalization_English
                // Using string.split to get "English"
                string language = _currentTextDatabase.Name.ToString().Split('_')[1];
                if(!language.Equals(_lastFrosTxtWindow?.language))
                {
                    // Check if FrosTxtWindow already in localizationProfiles
                    _searchTerm = language;
                    FrosTxtWindow searchResult = _localizationProfiles.Find(FrosTxtWindowPredicate);
                    if (searchResult != null)
                    {
                        // Set as last opened window
                        _lastFrosTxtWindow = searchResult;
                    }
                    else
                    {
                        // Create new window profile, add to localization profiles, and set as last opened window
                        // create base file with chunk stream
                        LocalizationFile baseFile = loadBaseChunk();
                        FrosTxtWindow newWindow = new FrosTxtWindow(baseFile, language, _currentTextDatabase, _currentLocalization);
                        _localizationProfiles.Add(newWindow);
                        _lastFrosTxtWindow = newWindow;
                    }
                }
            }
            // open last opened window
            _lastFrosTxtWindow.Show();
        }

        // Creates a new localization base file for the currently selected localization asset.
        // Pre-condition: the given FsUITextDatabase asset must be an open tab.
        private static LocalizationFile loadBaseChunk()
        {
            Guid chunkID = _currentTextDatabase.BinaryChunk;
            ChunkAssetEntry textChunk = App.AssetManager.GetChunkEntry(chunkID);
            Stream memStream = App.AssetManager.GetChunk(textChunk);
            FileStream baseTextStream = new FileStream(_tempChunk, FileMode.OpenOrCreate);
            memStream.CopyTo(baseTextStream);
            return new LocalizationFile(baseTextStream, _currentTextDatabase.Language.ToString());
        }

        private static bool CompareByAssetEntryName(AssetEntry a)
        {
            return a.Name == _searchTerm;
        }

        private static bool CompareByFrosTxtWindowLanguage(FrosTxtWindow f)
        {
            return f.language == _searchTerm;
        }

        // Open FrosTxt reversion window.
        public static void OnRevertFrosTxtCommand(object sender, RoutedEventArgs e)
        {

        }
    }
}

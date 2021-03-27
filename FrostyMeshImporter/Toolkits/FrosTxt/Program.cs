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
using FrostySdk.IO;
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
    // Enumaration of all localization types
    public enum Localizations
    {
        English,
        BrazilianPortuguese,
        French,
        German,
        Italian,
        Japanese,
        Polish,
        Russian,
        Spanish,
        SpanishMex,
        TraditionalChinese,
        WorstCase,
        Custom
    }

    partial class Program
    {
        // Container for all the data required to create a FrosTxt window and modify a specific
        // localization file.
        internal class FrosTxtObj
        {
            // The localization's corresponding language.
            public Localizations language;
            // The object that contains the asset's data fields, from current UI tab.
            public FsUITextDatabase localizationTextData;
            // The selected asset in the main window explorer.
            public EbxAssetEntry localizationAsset;
            // The object that controls editing of ebx assets.
            public FrostyAssetEditor localizationAssetEditor;
            // The FrosTxt object that facilitates merging localization files
            public LocalizationMerger lm;

            public FrosTxtObj(string language, FsUITextDatabase localizationTextData, EbxAssetEntry localizationAsset, 
                FrostyAssetEditor localizationAssetEditor, LocalizationMerger lm)
            {
                this.language = (Localizations)Enum.Parse(typeof(Localizations), language);
                this.localizationTextData = localizationTextData;
                this.localizationAsset = localizationAsset;
                this.localizationAssetEditor = localizationAssetEditor;
                this.lm = lm;
            }
        }

        private static List<FrosTxtObj> _localizationProfiles;
        private static EbxAssetEntry _currentLocalizationAsset;
        private static FsUITextDatabase _currentTextDatabase;
        private static FrosTxtObj _lastFrosTxtWindow;
        private static bool _openFrosTxt = false;
        private static string DEFAULT_LOCALIZATION_PATH = "Localization/WSLocalization_";
        private static string _tempPath= ".\\FrosTxtTemp";

        // Setup globals for FrosTxt window to be opened.
        public static void OnFrosTxtCommand(object sender, RoutedEventArgs e)
        {
            if(_localizationProfiles == null)
            {
                _localizationProfiles = new List<FrosTxtObj>();
            }
            if(!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
            if(_mainWindowExplorer.SelectedAsset != null && 
                _mainWindowExplorer.SelectedAsset.Type.Equals("FsUITextDatabase"))
            {
                // Open FrosTxt window corresponding to current selected asset
                _currentLocalizationAsset = (EbxAssetEntry)_mainWindowExplorer.SelectedAsset;
            } else if(_lastFrosTxtWindow != null)
            {
                // Open last FrosTxt window
                _currentLocalizationAsset = null;
            } else  // lastFrosTxtWindow is null, open default english
            {
                EbxAssetEntry englishLocalization = App.AssetManager.GetEbxEntry(DEFAULT_LOCALIZATION_PATH + "English");
                _mainWindowExplorer.SelectedAsset = englishLocalization;
                _currentLocalizationAsset = englishLocalization;
            }
            if(_currentLocalizationAsset == null)
            {
                // Opening last window
                OpenFrosTxtWindow();
                return;
            } else
            {
                // Example: data.Name = WSLocalization_English
                // Using string.split to get "English"
                string language = _mainWindowExplorer.SelectedAsset.Name.Split('_')[1];
                // Check if window to open is same as last
                if(_lastFrosTxtWindow?.language.ToString() == language)
                {
                    // Open last window
                    _currentLocalizationAsset = null;
                    OpenFrosTxtWindow();
                    return;
                }
                // If not opening last window, check if profile has already been created
                FrosTxtObj searchResult = GetFrosTxtProfile(language);
                if (searchResult != null)
                {
                    OpenFrosTxtWindow(searchResult);
                    return;
                }
                // Create new profile
                FsUITextDatabase testTab = _currentAssetEditor?.RootObject as FsUITextDatabase;
                if (testTab != null && _mainWindowExplorer.SelectedAsset != null &&
                    testTab.Language.ToString().Split('_')[1] == _mainWindowExplorer.SelectedAsset.Name.Split('_')[1])
                {
                    OpenFrosTxtWindow();
                    return;
                }
                else
                {
                    _mainWindowExplorer.DoubleClickSelectedAsset();
                    _openFrosTxt = true;
                }
            }
        }

        // Opens FrosTxt window specified by lastFrosTxtWindow unless given a specific FrosTxt profile to open.
        private static async void OpenFrosTxtWindow(FrosTxtObj windowToOpen = null)
        {
            if(windowToOpen != null)
            {
                // Open the given FrosTxt window instead of selected asset
                _lastFrosTxtWindow = windowToOpen;
            } else if (_currentLocalizationAsset != null)
            {
                // Open FrosTxtWindow using the current open localization asset
                // Create new window profile, add to localization profiles, and set as last opened window
                // create base file with chunk stream
                _currentTextDatabase = (FsUITextDatabase)_currentAssetEditor.RootObject;
                string language = _mainWindowExplorer.SelectedAsset.Name.Split('_')[1];
                FrostyTask.Begin($"Creating new FrosTxt profile");
                await Task.Run(() =>
                {
                    LocalizationFile baseFile = LoadBaseChunk();
                    LocalizationMerger lm = new LocalizationMerger(baseFile);
                    FrosTxtObj newWindow = new FrosTxtObj(language, _currentTextDatabase, _currentLocalizationAsset, _currentAssetEditor, lm);
                    _localizationProfiles.Add(newWindow);
                    _lastFrosTxtWindow = newWindow;
                });
                FrostyTask.End();
            }
            // open last opened window
            FrosTxtWindow toOpen = new FrosTxtWindow(_lastFrosTxtWindow);
            bool? result = toOpen.ShowDialog();
            if(result != true)
            {
                return;
            }
        }

        // Creates a new localization base file for the currently selected localization asset.
        // Pre-condition: the given FsUITextDatabase asset must be an open tab.
        private static LocalizationFile LoadBaseChunk()
        {
            Guid chunkID = _currentTextDatabase.BinaryChunk;
            ChunkAssetEntry textChunk = App.AssetManager.GetChunkEntry(chunkID);
            Stream memStream = App.AssetManager.GetChunk(textChunk);
            FileStream baseTextStream = new FileStream(_tempPath + "\\chunk.chunk", FileMode.OpenOrCreate);
            memStream.CopyTo(baseTextStream);
            LocalizationFile baseFile = new LocalizationFile(baseTextStream, _currentTextDatabase.Language.ToString());
            baseTextStream.Close();
            File.Delete(_tempPath + "\\chunk.chunk");
            return baseFile;
        }

        public static void SwitchFrosTxtProfile(string switchToLanguage)
        {
            FrosTxtObj toOpen = GetFrosTxtProfile(switchToLanguage);
            if(toOpen != null)
            {
                OpenFrosTxtWindow(toOpen);
            } else
            {
                // Create new profile
                EbxAssetEntry selectedLocalization = App.AssetManager.GetEbxEntry(DEFAULT_LOCALIZATION_PATH + switchToLanguage);
                _mainWindowExplorer.SelectedAsset = selectedLocalization;
                _currentLocalizationAsset = selectedLocalization;
                FsUITextDatabase testTab = _currentAssetEditor?.RootObject as FsUITextDatabase;
                if (testTab != null && _mainWindowExplorer.SelectedAsset != null &&
                    testTab.Language.ToString().Split('_')[1] == _mainWindowExplorer.SelectedAsset.Name.Split('_')[1])
                {
                    OpenFrosTxtWindow();
                }
                else
                {
                    _openFrosTxt = true;
                    _mainWindowExplorer.DoubleClickSelectedAsset();
                }
            }
        }

        // Merges localization files staged for merge in the localization profile stored in _lastFrosTxtWindow
        public static bool MergeCurrentProfile()
        {
            if(_lastFrosTxtWindow == null)
            {
                // Error failed to merge
                return false;
            }
            Cursor.Current = Cursors.WaitCursor;
            LocalizationMerger lm = _lastFrosTxtWindow.lm;
            _currentTextDatabase = _lastFrosTxtWindow.localizationTextData;
            _currentAssetEditor = _lastFrosTxtWindow.localizationAssetEditor;
            Guid chunkID = _currentTextDatabase.BinaryChunk;
            // Merge files and write to disk
            string outPath = $"{_tempPath}\\{chunkID}.chunk";
            lm.MergeFiles(outPath);
            // Read merged file from disk
            FileInfo mergedFileInfo = new FileInfo(outPath);
            long fileLen = mergedFileInfo.Length;
            using (NativeReader nativeReader = new NativeReader(new FileStream(outPath, FileMode.Open, FileAccess.Read)))
            {
                byte[] chunkData = nativeReader.ReadToEnd();
                App.AssetManager.ModifyChunk(chunkID, chunkData);
            }
            _currentTextDatabase.BinaryChunkSize = (uint)fileLen;
            _currentAssetEditor.AssetModified = true;
            File.Delete(outPath);
            Cursor.Current = Cursors.Default;
            return true;
        }

        // Reverts the profile corresponding with the given language to the initialized state
        public static void RevertProfile(FrosTxtObj toRevert)
        {
            Guid chunkID = toRevert.localizationTextData.BinaryChunk;
            ChunkAssetEntry revertChunk = App.AssetManager.GetChunkEntry(chunkID);
            // Revert localization binary chunk
            App.AssetManager.RevertAsset(revertChunk);
            // Revert localization ebx file
            App.AssetManager.RevertAsset(toRevert.localizationAsset);
            // Remove all files from localization merger except the base file
            _lastFrosTxtWindow.lm.ClearModifiedFiles();
            _mainWindowExplorer.RefreshAll();
        }

        // On context menu revert click
        public static void ContextRevertProfile(object sender, RoutedEventArgs e)
        {
            string language = _mainWindowExplorer.SelectedAsset.Name.Split('_')[1];
            FrosTxtObj toRevert = GetFrosTxtProfile(language);
            if(toRevert != null)
            {
                RevertProfile(toRevert);
            }
        }

        // Returns the FrosTxtObj corresponding to the given language if it exists, otherwise returns null.
        private static FrosTxtObj GetFrosTxtProfile(string language)
        {
            if(_localizationProfiles == null || _localizationProfiles.Count == 0)
            {
                return null;
            }
            Predicate<FrosTxtObj> CompareByFrosTxtObjLanguage = delegate(FrosTxtObj f) { return f.language.ToString() == language; };
            FrosTxtObj searchResult = _localizationProfiles.Find(CompareByFrosTxtObjLanguage);
            return searchResult;
        }

        private static bool CompareByAssetEntryName(AssetEntry a)
        {
            return a.Name == _searchTerm;
        }

        // Open FrosTxt reversion window.
        public static void OnRevertFrosTxtCommand(object sender, RoutedEventArgs e)
        {

        }
    }
}

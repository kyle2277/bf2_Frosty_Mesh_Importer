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
        internal class FrosTxtObj
        {
            public Localizations language;
            public FsUITextDatabase localizationTextData;
            public EbxAssetEntry localizationAsset;
            public LocalizationMerger lm;

            public FrosTxtObj(string language, FsUITextDatabase localizationTextData, EbxAssetEntry localizationAsset, LocalizationMerger lm)
            {
                this.language = (Localizations)Enum.Parse(typeof(Localizations), language);
                this.localizationTextData = localizationTextData;
                this.localizationAsset = localizationAsset;
                this.lm = lm;
            }
        }

        private static List<FrosTxtObj> _localizationProfiles;
        private static EbxAssetEntry _currentLocalizationAsset;
        private static FsUITextDatabase _currentTextDatabase;
        private static FrosTxtObj _lastFrosTxtWindow;
        private static bool _openFrosTxt = false;
        private static string DEFAULT_LOCALIZATION_PATH = "Localization/WSLocalization_";
        private static string _tempChunk = ".\\FrosTxtTemp\\chunk.chunk";

        // Setup globals for FrosTxt window to be opened.
        public static void OnFrosTxtCommand(object sender, RoutedEventArgs e)
        {
            if(_localizationProfiles == null)
            {
                _localizationProfiles = new List<FrosTxtObj>();
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
                _searchTerm = language;
                Predicate<FrosTxtObj> FrosTxtWindowPredicate = CompareByFrosTxtWindowLanguage;
                FrosTxtObj searchResult = _localizationProfiles.Find(FrosTxtWindowPredicate);
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
        private static void OpenFrosTxtWindow(FrosTxtObj windowToOpen = null)
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
                LocalizationFile baseFile = LoadBaseChunk();
                LocalizationMerger lm = new LocalizationMerger(baseFile);
                string language = _mainWindowExplorer.SelectedAsset.Name.Split('_')[1];
                FrosTxtObj newWindow = new FrosTxtObj(language, _currentTextDatabase, _currentLocalizationAsset, lm);
                _localizationProfiles.Add(newWindow);
                _lastFrosTxtWindow = newWindow;
            }
            // open last opened window
            FrosTxtWindow toOpen = new FrosTxtWindow(_lastFrosTxtWindow);
            bool? result = toOpen.ShowDialog();
            if(result != true)
            {
                App.Logger.Log($"Closed {toOpen.Title}.");
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
            FileStream baseTextStream = new FileStream(_tempChunk, FileMode.OpenOrCreate);
            memStream.CopyTo(baseTextStream);
            return new LocalizationFile(baseTextStream, _currentTextDatabase.Language.ToString());
        }

        public static void SwitchFrosTxtProfile(string switchToLanguage)
        {
            _searchTerm = switchToLanguage;
            Predicate<FrosTxtObj> FrosTxtWindowPredicate = CompareByFrosTxtWindowLanguage;
            FrosTxtObj toOpen = _localizationProfiles.Find(FrosTxtWindowPredicate);
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

        private static bool CompareByAssetEntryName(AssetEntry a)
        {
            return a.Name == _searchTerm;
        }

        private static bool CompareByFrosTxtWindowLanguage(FrosTxtObj f)
        {
            return f.language.ToString() == _searchTerm;
        }

        // Open FrosTxt reversion window.
        public static void OnRevertFrosTxtCommand(object sender, RoutedEventArgs e)
        {

        }
    }
}

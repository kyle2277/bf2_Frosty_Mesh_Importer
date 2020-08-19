using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frosty.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using System.IO;
using System.Runtime.CompilerServices;

// <summary>
// Author: Kyle Won
// Adds Res/Chunk file batch import funtionality to the Frosty Mod Editor.
// </summary>

namespace FrostyResChunkImporter
{
    public enum errorState
    {
        Success = 0,
        ChunkFileNotFound = -1,
        ResFileNotFound = -2,
        UnableToRefreshExplorer = -3,
        NoResFileSelected = -4,
        CannotOverwriteExistingFile = -5
    };

    class Program
    {
        private static MainWindow _mainWindow;
        private static FrostyAssetEditor _currentAssetEditor;
        private static FrostyChunkResExplorer _chunkResExplorer;
        private static FrostyDataExplorer _resExplorer;
        private static FrostyTabControl _tabControl;
        private static App _app;
        private static string _version;

        [STAThread]
        static void Main(string[] args)
        {
            var altDomain = AppDomain.CreateDomain("FrostyResChunkImporter");
            altDomain.Load(typeof(App).Assembly.GetName());
            altDomain.DoCallBack(() =>
            {
                Application.ResourceAssembly = typeof(App).Assembly;
                _app = new App();
                _app.Activated += OnAppActivated;
                _app.InitializeComponent();
                _app.Run();
            });
        }

        private static void OnAppActivated(object sender, EventArgs e)
        {
            if (_app.MainWindow is FrostyEditor.Windows.PrelaunchWindow prelaunchWindow)
            {
                prelaunchWindow.Closed += (object sender2, EventArgs e2) =>
                {
                    if (_app.MainWindow is FrostyEditor.Windows.SplashWindow splashWindow)
                    {
                        splashWindow.Closed += (object sender3, EventArgs e3) =>
                        {
                            if (_app.MainWindow is MainWindow mainWindow)
                            {
                                _mainWindow = mainWindow;
                                OnMainWindowLaunch(mainWindow);
                            }
                        };
                    }
                };
                _app.Activated -= OnAppActivated;
            }
        }

        private static void OnMainWindowLaunch(MainWindow mainWindow)
        {
            _version = typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            App.Logger.Log($"Frosty Res/Chunk Importer - Version {_version}");

            // hack because Frosty calls GetEntryAssembly() which returns null normally
            var domainManager2 = new AppDomainManager();
            domainManager2.SetFieldValue("m_entryAssembly", Application.ResourceAssembly);
            AppDomain.CurrentDomain.SetFieldValue("_domainManager", domainManager2);
            
            // Inject functionality
            _tabControl = mainWindow.GetFieldValue<FrostyTabControl>("tabControl");
            _tabControl.SelectionChanged += TabControlOnSelectionChanged;
        }

        private static void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if user has switched to asset viewer tab
            var selectedTab = _tabControl.SelectedItem as FrostyTabItem;
            if(!(selectedTab?.Content is FrostyAssetEditor assetEditorContent))
            {
                // If not an asset viewer tab, do nothing
                return;
            }
            _currentAssetEditor = assetEditorContent;
            // Inject new functions into asset viewer toolbar
            var control = FindChild<ItemsControl>(_mainWindow, "editorToolbarItems");
            var items = _currentAssetEditor.RegisterToolbarItems();
            items.Add(new ToolbarItem("Import Mesh", "Import mesh or cloth asset's res/chunk files", "Images/Import.png", new RelayCommand(_ => OnImporterCommand(false),_ => true)));
            items.Add(new ToolbarItem("Revert Mesh", "Revert a mesh or cloth asset", "Images/Revert.png", new RelayCommand(_ => OnImporterCommand(true), _ => true)));
            items.Add(new ToolbarItem("Export Res", "Export selected res file in res explorer", "Images/Export.png", new RelayCommand(_ => OnExportCommand(), _ => true)));
            control.ItemsSource = items;
        }

        private static bool hasChunkResExplorer()
        {
            // refreshes and then retrieves the active res/chunk explorer. Return true if exists, otherwise, logs and return false.
            _chunkResExplorer = null;
            var opentabs = _tabControl.Items;
            foreach (FrostyTabItem tab in opentabs)
            {
                if (tab?.Content is FrostyChunkResExplorer chunkResExplorerContent)
                {
                    _chunkResExplorer = chunkResExplorerContent;
                }
            }

            if (_chunkResExplorer == null)
            {
                App.Logger.Log("ERROR: No active Res/Chunk Explorer found. Open the Res/Chunk Explorer from the Tools dropdown menu and re-execute order.");
                return false;
            }
            else
            {
                FrostyDataExplorer resExplorer = ReflectionHelper.GetFieldValue<FrostyDataExplorer>(_chunkResExplorer, "resExplorer");
                return true;
            }
        }

        private static async void OnImporterCommand(bool revert)
        {
            string operation = revert ? "revert" : "import";
            // Check if there is an open res/chunk explorer. If not, exit operation
            if (!hasChunkResExplorer())
            {
                return;
            }
            App.Logger.Log($"{operation.Substring(0,1).ToUpper()}{operation.Substring(1, operation.Length - 1)} will commence shortly.");

            // Get path to res/chunk data directory
            OpenFileDialog ofd = new OpenFileDialog();
            // Config dialog
            ofd.Title = "Navigate to Res/Chunk folder";
            ofd.Filter = "Folder|*.*";
            ofd.RestoreDirectory = true;
            ofd.AddExtension = false;
            ofd.ValidateNames = false;
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = true;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = false;
            ofd.FilterIndex = 2;
            string ofdPrompt = "Folder";
            ofd.FileName = ofdPrompt;

            // Open dialog
            // If user cancels file dialog, log and exit operation
            if (ofd.ShowDialog(_mainWindow) != true) 
            {
                App.Logger.Log($"Canceled {operation}.");
                return;
            }

            // UI reference used to refresh the res explorer
            FrostyDataExplorer resExplorer = null;
            
            string task = revert ? "Reverting asset" : "Importing res/chunk files";
            FrostyTask.Begin(task);
            await Task.Run(() =>
            {
            // Remove file name appended by open file dialog
            string ofdResult = ofd.FileName.Remove(ofd.FileName.Length - ofdPrompt.Length, ofdPrompt.Length);
            App.Logger.Log($"File selected: {ofdResult}");

            //Check if the selected file is a directory, if not, log and exit operation
            FileAttributes attr = File.GetAttributes(ofdResult);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                App.Logger.Log("ERROR: Selected file is not a folder.");
                return;
            }

            // Sort res and chunk files into lists, if a file is not a res or chunk file, log and exit operation
            List<string> allFiles = Directory.EnumerateFiles(ofdResult).ToList();
            List<ChunkResFile> chunkFiles = new List<ChunkResFile>();
            List<ChunkResFile> resFiles = new List<ChunkResFile>();

            foreach (string absolutePath in allFiles)
            {
                string extension = Path.GetExtension(absolutePath);
                string tempFileName = Path.GetFileName(absolutePath);
                string parentDir = Path.GetDirectoryName(absolutePath);
                // Remove extension from file name
                string fileName = tempFileName.Remove(tempFileName.Length - extension.Length, extension.Length);
                ChunkResFile curFile = new ChunkResFile(absolutePath, parentDir, fileName, extension, null);
                switch (extension)
                {
                    case ".chunk":
                        chunkFiles.Add(curFile);
                        break;
                    case ".res":
                        resFiles.Add(curFile);
                        break;
                    default:
                        App.Logger.Log($"ERROR: Non-chunk or res file found. Canceled {operation}.");
                        return;
                }
            }

                ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, chunkFiles, resFiles);
                resExplorer = importer.Import(revert, ofdResult);
                // Set importer to null for garbage collection
                importer = null;
            });
            FrostyTask.End();
            
            // Refresh chunk explorer
            ChunkAssetEntry[] chunkParams = new ChunkAssetEntry[] { null };
            ReflectionHelper.InvokeMethod(_chunkResExplorer, "RefreshChunksListBox", chunkParams);
            // Refresh res explorer
            if(resExplorer != null)
            {
                ReflectionHelper.InvokeMethod(resExplorer, "RefreshItems", null);
            }
            else
            {
                App.Logger.Log("ERROR: Could not refresh the Res Explorer. Missing reference.");
            }
            // Success
            App.Logger.Log($"{operation.Substring(0, 1).ToUpper()}{operation.Substring(1, operation.Length - 1)} Successful!");
        }

        private static async void OnExportCommand()
        {
            // Check if there is an open res/chunk explorer. If not, exit operation
            if (!hasChunkResExplorer())
            {
                return;
            }
            ResAssetEntry selectedAsset = null;
            // Use dispatcher to get access to UI element selected asset
            _mainWindow.Dispatcher.Invoke((Action)(() =>
            {
                selectedAsset = _resExplorer.SelectedAsset as ResAssetEntry;
            }));
            if (selectedAsset == null)
            {
                App.Logger.Log($"ERROR: {errorState.NoResFileSelected}. Cancelling export.");
                return;
            }
            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save Resource", "*.res (Resource Files)|*.res", "Res", selectedAsset.Filename, true);
            Stream resStream = App.AssetManager.GetRes(selectedAsset);
            if (resStream == null || !sfd.ShowDialog())
            {
                App.Logger.Log($"ERROR: {errorState.ResFileNotFound}. Cancelling export.");
                return;
            }
            // Check if file already exists, if so, exit operation
            string selectedFile = sfd.FileName;
            if (File.Exists(selectedFile))
            {
                App.Logger.Log($"ERROR: {errorState.CannotOverwriteExistingFile}. Cancelling export.");
                return;
            }

            FrostyTask.Begin("Exporting res file");
            await Task.Run(() =>
            {
                ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, null, null);
                int status = importer.ExportResFile(selectedAsset, selectedFile);
                if(status < 0)
                {
                    App.Logger.Log($"ERROR: {(errorState)status}. Cancelling export.");
                    return;
                }
                App.Logger.Log("Export successful!");
                // set importer to null for garbage collection
                importer = null;
            });
            FrostyTask.End();
        }

        /// <summary>
        /// Finds a child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);

                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}

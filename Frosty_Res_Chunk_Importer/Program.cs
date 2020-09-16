// Program.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
//      Copyright (C) 2020  Daniel Elam <dan@dandev.uk>
// This file is subject to the terms and conditions defined in the 'LICENSE' file.
// The following code is derived from Daniel Elam's bf2-sound-import project

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
using Frosty.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Managers;
using Microsoft.Win32;
using System.IO;
using FrostySdk.Ebx;
using System.Web.SessionState;
using FrostyResChunkImporter.Windows;

// <summary>
// Adds Res/Chunk file batch import funtionality to the Frosty Mod Editor.
// </summary>

namespace FrostyResChunkImporter
{
    public enum errorState
    {
        Success = 0,
        ChunkFileNotFound = -1,
        CriticalResFileError = -2,
        UnableToRefreshExplorer = -3,
        NoResFileSelected = -4,
        CannotOverwriteExistingFile = -5,
        NonCriticalResFileError = -6,
        NonNominalReturn = -7,
        NoActiveResChunkExplorer = -8,
        SelectedFileIsNotFolder = -9,
        SelectedFolderIsEmpty = -10,
        NonChunkResFileFound = -11,
        MissingResID = -12,
        NoImportedAssets = -13,
        NoFrostMeshySourceLinked = -14,
        PathDoesNotExist = -15
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
        private static string _searchTerm;
        private static string _fMeshySrcDir;
        public const string IMPORTER_ERROR = "Frosty Mesh Importer Error";
        public const string IMPORTER_WARNING = "Frosty Mesh Importer Warning";
        public const string IMPORTER_MESSAGE = "Frosty Mesh Importer Message";

        [STAThread]
        static void Main(string[] args)
        {
            var altDomain = AppDomain.CreateDomain("FrostyResChunkImporter");
            altDomain.Load(typeof(App).Assembly.GetName());
            altDomain.DoCallBack(() =>
            {
                System.Windows.Application.ResourceAssembly = typeof(App).Assembly;
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
            App.Logger.Log($"TigerVenom22's Frosty Mesh Importer - Version {_version}");

            // hack because Frosty calls GetEntryAssembly() which returns null normally
            var domainManager2 = new AppDomainManager();
            domainManager2.SetFieldValue("m_entryAssembly", System.Windows.Application.ResourceAssembly);
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
            items.Add(new ToolbarItem("   ", null, null, new RelayCommand((Action<object>)(state => { }), (Predicate<object>)(state => false))));
            items.Add(new ToolbarItem("Import Mesh", "Import mesh or cloth asset's res/chunk files", "Images/Import.png", new RelayCommand(_ => OnImporterCommand(false),_ => true)));
            items.Add(new ToolbarItem("Revert Mesh", "Revert a mesh or cloth asset", "Images/Revert.png", new RelayCommand(_ => OnImporterCommand(true), _ => true)));
            items.Add(new ToolbarItem("Export Res", "Export selected res file in res explorer", "Images/Export.png", new RelayCommand(_ => OnExportCommand(), _ => true)));
            items.Add(new ToolbarItem("   ", null, null, new RelayCommand((Action<object>)(state => { }), (Predicate<object>)(state => false))));
            items.Add(new ToolbarItem("Link Source", "Link to the output folder of FrostMeshy", "Images/Interface.png", new RelayCommand(_ => OnLinkSourceCommand(), _ => true)));
            items.Add(new ToolbarItem("Source Import", "Import mesh or cloth asset from FrostMeshy output folder", "Images/Import.png", new RelayCommand(_ => OnSourceImportCommand(), _ => true)));
            items.Add(new ToolbarItem("   ", null, null, new RelayCommand((Action<object>)(state => { }), (Predicate<object>)(state => false))));
            items.Add(new ToolbarItem("History", "Re-import or revert mesh sets from history", "Images/Classref.png", new RelayCommand(_ => OnHistoryCommand(), _ => true)));
            items.Add(new ToolbarItem("   ", null, null, new RelayCommand((Action<object>)(state => { }), (Predicate<object>)(state => false))));
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
                Log(errorState.NoActiveResChunkExplorer.ToString(), "Res/Chunk Explorer not found. " +
                    "Open the Res/Chunk Explorer from the Tools dropdown menu and re-execute order.", MessageBoxButton.OK, IMPORTER_ERROR);
                return false;
            }
            else
            {
                _resExplorer = ReflectionHelper.GetFieldValue<FrostyDataExplorer>(_chunkResExplorer, "resExplorer");
                return true;
            }
        }
        
        //user clicks Import/Revert mesh
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
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            // Config dialog
            ofd.Title = "Navigate to mesh set folder";
            ofd.Filter = "Folder|*.*";
            ofd.RestoreDirectory = true;
            ofd.AddExtension = false;
            ofd.ValidateNames = false;
            ofd.CheckFileExists = false;
            ofd.CheckPathExists = true;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = true;
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
            
            string task = revert ? "Reverting asset" : "Importing res/chunk files";
            FrostyTask.Begin(task);
            await Task.Run(() =>
            {
                // Remove file name appended by open file dialog
                string fullPath = ofd.FileName;
                string fileName = Path.GetFileName(fullPath);
                string dirPath = Path.GetDirectoryName(fullPath);   
                int status = FileOperation(dirPath, operation, revert);
                if (status < (int)errorState.Success)
                {
                    App.Logger.Log($"ERROR: {(errorState)status}");
                    return;
                }
                if(status > (int)errorState.Success)
                {
                    FrostyMessageBox.Show($"{status} Res files must be {operation}ed manually. See log for details.", IMPORTER_WARNING, MessageBoxButton.OK);
                }
                // Success
                App.Logger.Log($"{operation.Substring(0, 1).ToUpper()}{operation.Substring(1, operation.Length - 1)} Successful!");
            });
            FrostyTask.End();
            RefreshExplorers();
        }

        private static int MultiFileImport(List<string> dirNames, string operation, bool revert)
        {
            int status = 0;
            foreach(string folder in dirNames)
            {
                string path = _fMeshySrcDir + "\\" + folder;
                if(!Directory.Exists(path))
                {
                    Log(errorState.PathDoesNotExist.ToString(), $"Folder at {path} does not exist.", MessageBoxButton.OK, IMPORTER_ERROR);
                }
                else
                {
                    int result = FileOperation(path, operation, revert);
                    if (result < 0)
                    {
                        App.Logger.Log($"ERROR: {(errorState)result}");
                    }
                    else if (result > 0)
                    {
                        status += result;
                    }
                }
            }
            return status;
        }

        private static int FileOperation(string dirPath, string operation, bool revert)
        {
            // Parse mesh set name (name of directory)
            string[] pathSplit = dirPath.Split('\\');
            string meshSetName = pathSplit[pathSplit.Length - 1];
            App.Logger.Log($"Folder selected: {dirPath}");
            //Check if the selected file is a directory, if not, log and exit operation
            FileAttributes attr = File.GetAttributes(dirPath);
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                Log(errorState.SelectedFileIsNotFolder.ToString(), $"The selected file must be a folder. Canceled {operation} on mesh " +
                    $"set {meshSetName}.", MessageBoxButton.OK, IMPORTER_ERROR);
                return (int)errorState.SelectedFileIsNotFolder;
            }
            // Sort res and chunk files into lists, if a file is not a res or chunk file or if the folder is empty, log and exit operation
            List<string> allFiles = Directory.EnumerateFiles(dirPath).ToList();
            List<ChunkResFile> chunkFiles = new List<ChunkResFile>();
            List<ChunkResFile> resFiles = new List<ChunkResFile>();
            int popStatus;
            if ((popStatus = PopulateChunkResLists(operation, allFiles, chunkFiles, resFiles)) < 0)
            {
                return popStatus;
            }
            ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, chunkFiles, resFiles, meshSetName, dirPath);
            // Positive status indicates number of res files to be imported manually
            int status = importer.Import(revert);
            // Set importer to null for garbage collection
            importer = null;
            return status;
        }

        // Adds chunk and res files to lists, returns -1 if error
        private static int PopulateChunkResLists(string operation, List<string> allFiles, List<ChunkResFile> chunkFiles, List<ChunkResFile> resFiles)
        {
            if (allFiles.Count == 0)
            {
                Log(errorState.SelectedFolderIsEmpty.ToString(), $"The selected folder has no files in it. Canceled {operation}.", MessageBoxButton.OK, IMPORTER_ERROR);
                return (int)errorState.SelectedFolderIsEmpty;
            }

            foreach (string absolutePath in allFiles)
            {
                string extension = Path.GetExtension(absolutePath);
                ChunkResFile curFile = new ChunkResFile(absolutePath, null);
                switch (extension)
                {
                    case ".chunk":
                        chunkFiles.Add(curFile);
                        break;
                    case ".res":
                        resFiles.Add(curFile);
                        break;
                    default:
                        Log(errorState.NonChunkResFileFound.ToString(),
                            $"Folder contains files which are not .chunk or .res files. Canceled {operation}.", MessageBoxButton.OK, IMPORTER_ERROR);
                        return (int)errorState.NonChunkResFileFound;
                }
            }
            return 0;
        }

        //user clicks "Export Res File"
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
                Log(errorState.NoResFileSelected.ToString(), "No Res file selected in the Res explorer. Canceled export.", MessageBoxButton.OK, IMPORTER_ERROR);
                return;
            }

            //export path dialog
            string selectedFile;
            do
            {
                FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save Resource", "*.res (Resource Files)|*.res", "Res", selectedAsset.Filename, true);
                //if user exits dialog, log and exit operation
                if (!sfd.ShowDialog())
                {
                    App.Logger.Log("Canceled export.");
                    return;
                }

                // Check if file already exists, if so, exit operation
                selectedFile = sfd.FileName;
                if (!File.Exists(selectedFile))
                {
                    break;
                }
                MessageBoxResult result = Log(errorState.CannotOverwriteExistingFile.ToString(), "Cannot save over an existing file. " +
                    "Would you like to choose a different path?", MessageBoxButton.OKCancel, IMPORTER_ERROR);
                if (result == MessageBoxResult.Cancel)
                {
                    App.Logger.Log("Canceled export.");
                    return;
                }
            } while (File.Exists(selectedFile));

            FrostyTask.Begin("Exporting res file");
            await Task.Run(() =>
            {
                ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, null, null, null, null);
                int status = importer.ExportResFile(selectedAsset, selectedFile);
                if(status < 0)
                {
                    App.Logger.Log($"ERROR: {(errorState)status}. Canceled export.");
                    return;
                }
                App.Logger.Log("Export successful!");
                // set importer to null for garbage collection
                importer = null;
            });
            FrostyTask.End();
            RefreshExplorers();
        }

        //user clicks "Revert Imported Mesh"
        private static async void OnHistoryCommand()
        {
            //Check if the importer has any imported meshes saved, if not, log and exit
            if (ChunkResImporter.importedAssets == null || ChunkResImporter.importedAssets.Count == 0)
            {
                Log(errorState.NoImportedAssets.ToString(), $"No imported mesh sets in history.", MessageBoxButton.OK, IMPORTER_ERROR);
                return;
            }
            if (!hasChunkResExplorer())
            {
                return;
            }
            HistoryWindow hw = new HistoryWindow();
            hw.ShowDialog();
            if (hw.DialogResult == false)
            {
                App.Logger.Log($"Canceled history operation.");
                return;
            }
            else if(hw.DialogResult == true)
            {
                bool revert = hw.revert;
                string operation = revert ? "revert" : "re-import";
                App.Logger.Log($"Batch {operation} will commence shortly.");
                FrostyTask.Begin($"{operation.Substring(0, 1).ToUpper()}{operation.Substring(1, operation.Length - 1)}ing selected assets");
                await Task.Run(() =>
                {
                    Predicate<ImportedAsset> selectedPredicate = CompareByName;
                    List<string> selectedItems = hw.selectedItems;
                    int resCounter = 0;
                    foreach (string item in selectedItems)
                    {
                        _searchTerm = item.ToString();
                        ImportedAsset curAsset = ChunkResImporter.importedAssets.Find(selectedPredicate);
                        int status;
                        if(revert)
                        {
                            bool? removeReverted = new bool?(false);
                            // Use dispatcher to get access to UI element selected asset
                            _mainWindow.Dispatcher.Invoke((Action)(() =>
                            {
                                removeReverted = hw.revertCheckBox.IsChecked;
                            }));
                            status = Program.InternalRevert(curAsset, removeReverted); ;
                        } 
                        else
                        {
                            status = Program.InternalImport(curAsset, null);
                        }
                        if(status < 0)
                        {
                            App.Logger.Log($"ERROR: {(errorState)status}.");
                        }
                        else if (status > 0)
                        {
                            resCounter += status;
                        }
                    }
                    App.Logger.Log($"Successfully {operation}ed assets!");
                    if (resCounter > 0)
                    {
                        FrostyMessageBox.Show($"{resCounter} Res files need to be {operation}ed manually. See log for details.", Program.IMPORTER_WARNING, MessageBoxButton.OK);
                    }
                });
                FrostyTask.End();
                RefreshExplorers();
            }
        }

        private static bool CompareByName(ImportedAsset f)
        {
            return f.meshSetName == _searchTerm;
        }

        // Helper to perform revert from recorded import instead of from file
        private static int InternalRevert(ImportedAsset asset, bool? removeReverted)
        {
            ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, asset.chunks, asset.res, asset.meshSetName, asset.directory);
            importer.SetRemoveReverted(removeReverted);
            int status = importer.Import(revert: true);
            importer = null;
            return status;
        }

        // Helper to perform re-import from recorded imports, returns -1 if error
        private static int InternalImport(ImportedAsset asset, string dir)
        {
            string srcDir;
            if(dir == null)
            {
                srcDir = asset.directory;
            }
            else
            {
                srcDir = dir;
            }
            if(!Directory.Exists(srcDir))
            {
                Log(errorState.PathDoesNotExist.ToString(), $"Path does not exist: {srcDir}. Canceled import for mesh set {asset.meshSetName}.", 
                    MessageBoxButton.OK, IMPORTER_ERROR);
                return (int)errorState.PathDoesNotExist;
            }
            else
            {
                List<string> allFiles = Directory.EnumerateFiles(srcDir).ToList();
                List<ChunkResFile> chunkFiles = new List<ChunkResFile>();
                List<ChunkResFile> resFiles = new List<ChunkResFile>();
                int status = PopulateChunkResLists("re-import", allFiles, chunkFiles, resFiles);
                if (status < 0)
                {
                    return status;
                }
                ChunkResImporter importer = new ChunkResImporter(_mainWindow, _chunkResExplorer, _resExplorer, chunkFiles, resFiles, asset.meshSetName, asset.directory);
                status = importer.Import(revert: false);
                importer = null;
                return status;
            }
        }

        private static void OnLinkSourceCommand()
        {
            if (_fMeshySrcDir != null && _fMeshySrcDir != "")
            {
                MessageBoxResult r = FrostyMessageBox.Show($"FrostMeshy output source is currently set to: {_fMeshySrcDir}. Do you want to change it?",
                    IMPORTER_MESSAGE, MessageBoxButton.YesNo);
                if (r == MessageBoxResult.No)
                {
                    App.Logger.Log("Link source canceled.");
                    return;
                }
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select FrostMeshy output folder";
            fbd.ShowNewFolderButton = false;
            DialogResult d = fbd.ShowDialog();
            if(d != DialogResult.OK)
            {
                App.Logger.Log("Link source canceled.");
                return;
            }            
            _fMeshySrcDir = fbd.SelectedPath;
            Log(errorState.Success.ToString(), $"Linked to FrostMeshy output folder: {_fMeshySrcDir}.", MessageBoxButton.OK, IMPORTER_MESSAGE);
        }

        private static async void OnSourceImportCommand()
        {
            if (_fMeshySrcDir == null)
            {
                Log(errorState.NoFrostMeshySourceLinked.ToString(), "No FrostMeshy output folder has been linked.",
                    MessageBoxButton.OK, IMPORTER_ERROR);
                return;
            }
            else if(!Directory.Exists(_fMeshySrcDir))
            {
                Log(errorState.PathDoesNotExist.ToString(), "Path to FrostMeshy output does not exist. Click \"Link Source\" to link new FrostMeshy output.", 
                    MessageBoxButton.OK, IMPORTER_ERROR);
                return;
            }
            if (!hasChunkResExplorer())
            {
                return;
            }
            List<string> strFiles = Directory.EnumerateDirectories(_fMeshySrcDir).ToList();
            List<object> files = new List<object>();
            foreach(string fullPath in strFiles)
            {
                string meshSetName = Path.GetFileName(fullPath);
                ImportedAsset curAsset = new ImportedAsset(meshSetName, _fMeshySrcDir, null, null);
                files.Add(curAsset);
            }
            App.Logger.Log($"Import from FrostMeshy output will commence shortly.");
            SourceImportWindow siw = new SourceImportWindow();
            siw.SetItems(files);
            siw.ShowDialog();
            if (siw.DialogResult == false)
            {
                App.Logger.Log("Canceled FrostMeshy import.");
                return;
            }
            else if (siw.DialogResult == true)
            {
                FrostyTask.Begin($"Importing selected assets");
                await Task.Run(() =>
                {
                    List<string> selectedAssets = siw.selectedItems;
                    int status = MultiFileImport(selectedAssets, "import", false);
                    if (status > (int)errorState.Success)
                    {
                        FrostyMessageBox.Show($"{status} Res files must be imported manually. See log for details.", IMPORTER_WARNING, MessageBoxButton.OK);
                    }
                    App.Logger.Log("Import successful!");
                });
                FrostyTask.End();
                RefreshExplorers();
            }
        }

        private static void RefreshExplorers()
        {
            // Refresh chunk explorer
            ChunkAssetEntry[] chunkParams = new ChunkAssetEntry[] { null };
            ReflectionHelper.InvokeMethod(_chunkResExplorer, "RefreshChunksListBox", chunkParams);
            // Refresh res explorer
            if (_resExplorer != null)
            {
                ReflectionHelper.InvokeMethod(_resExplorer, "RefreshItems", null);
            }
            else
            {
                Log(errorState.NonCriticalResFileError.ToString() + ": " + errorState.UnableToRefreshExplorer, 
                    "Unable to refresh the Res/Chunk explorer. Missing reference.", MessageBoxButton.OK, IMPORTER_WARNING);
            }
        }

        //Log helper writes errors to log and creates popup message window
        private static MessageBoxResult Log(string errorState, string message, MessageBoxButton buttons, string messageType)
        {
            string error;
            switch (messageType)
            {
                case IMPORTER_ERROR:
                    error = "ERROR";
                    break;
                case IMPORTER_WARNING:
                    error = "WARNING";
                    break;
                case IMPORTER_MESSAGE:
                    error = "MESSAGE";
                    break;
                default:
                    error = "MESSAGE";
                    break;
            }            
            App.Logger.Log($"{error}: {errorState}. {message}");
            return FrostyMessageBox.Show(message, messageType, buttons);
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

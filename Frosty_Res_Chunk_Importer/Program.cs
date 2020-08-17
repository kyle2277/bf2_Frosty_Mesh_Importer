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


namespace FrostyResChunkImporter
{
    class Program
    {
        private static MainWindow _mainWindow;
        private static FrostyAssetEditor _currentAssetEditor;
        private static FrostyChunkResExplorer _chunkResExplorer;
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
            items.Add(new ToolbarItem("Mesh Chunk Import", "Auto import mesh asset chunk files", "Images/Import.png", new RelayCommand(_ => OnImportChunkCommand(),_ => true)));
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
                App.Logger.Log("ERROR: No active Res/Chunk Explorer found. Open the Res/Chunk Explorer from the Tools dropdown menu and re-execute.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void OnImportChunkCommand()
        {
            // Check if there is an open res/chunk explorer. If not, exit operation
            if (!hasChunkResExplorer())
            {
                return;
            }
            
            App.Logger.Log("Chunk importing will commence shortly.");
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

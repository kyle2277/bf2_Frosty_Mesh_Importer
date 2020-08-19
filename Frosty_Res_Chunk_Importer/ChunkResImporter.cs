using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
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
using System.Collections.ObjectModel;
using System.CodeDom;

// <summary>
// Author: Kyle Won
// Facilitates res/chunk file imports within the Frosty res/chunk explorer window.
// </summary>

namespace FrostyResChunkImporter
{
    class ChunkResImporter
    {
        private static MainWindow _mainWindow;
        private static FrostyDataExplorer _resExplorer;
        private FrostyChunkResExplorer _chunkResExplorer;
        private static List<ChunkAssetEntry> _allChunks;
        private static List<ResAssetEntry> _allResFiles;
        private static List<ChunkResFile> _exportedResFiles;
        private List<ChunkResFile> _chunkFiles;
        private List<ChunkResFile> _resFiles;
        private string _searchTerm;
        private readonly string RES_DATA_PATH = Path.GetFullPath("./resdata.json");

        public ChunkResImporter(MainWindow mainWindow, FrostyChunkResExplorer chunkResExplorer, List<ChunkResFile> chunkFiles, List<ChunkResFile> resFiles)
        {
            _mainWindow = mainWindow;
            _chunkResExplorer = chunkResExplorer;
            _chunkFiles = chunkFiles;
            _resFiles = resFiles;
            _resExplorer = ReflectionHelper.GetFieldValue<FrostyDataExplorer>(chunkResExplorer, "resExplorer");
            if(_exportedResFiles == null)
            {
                _exportedResFiles = new List<ChunkResFile>();
            }
            // Index chunk/res explorer asset entries
            InitResChunkLists(_chunkResExplorer);
        }

        // Facilitates chunk imports from chunk files list. Takes boolean which denotes whether to import or revert files
        public FrostyDataExplorer Import(bool revert)
        {
            // Checks if chunk and res explorer need to be re-indexed
            if (_allChunks == null || _allResFiles == null)
            {
                InitResChunkLists(_chunkResExplorer);
            }
            Predicate<AssetEntry> predicate = CompareAssets;

            // Find and import chunks in Frosty chunk explorer
            foreach (ChunkResFile newChunk in _chunkFiles)
            {
                _searchTerm = newChunk.fileName;
                ChunkAssetEntry oldChunk = _allChunks.Find(predicate);
                if(revert) 
                { 
                    RevertAsset(oldChunk, true); 
                }
                else 
                { 
                    ImportChunk(oldChunk, newChunk);
                }
            }

            // Find and import res files in Frosty res explorer
            foreach(ChunkResFile newRes in _resFiles)
            {
                _searchTerm = newRes.fileName;
                ResAssetEntry oldRes = _allResFiles.Find(predicate);
                if(revert)
                {
                    RevertAsset(oldRes, false);
                }
                else
                {
                    ImportResFiles(oldRes, newRes);
                }
            }
            return _resExplorer;
        }

        // Imports a single chunk. Takes chunk to replace and path to new chunk
        private int ImportChunk(ChunkAssetEntry oldChunk, ChunkResFile newChunk)
        {        
            using (NativeReader nativeReader = new NativeReader(new FileStream(newChunk.absolutePath, FileMode.Open, FileAccess.Read)))
            {
                byte[] end = nativeReader.ReadToEnd();
                App.AssetManager.ModifyChunk(oldChunk.Id, end, (Texture)null);
            }
            App.Logger.Log($"Imported chunk file: {newChunk.fileName}");
            return (int)errorState.Success;
        }

        // Import res file from res files list
        public int ImportResFiles(ResAssetEntry oldRes, ChunkResFile newRes)
        {
            using (NativeReader nativeReader = new NativeReader((System.IO.Stream)new FileStream(newRes.absolutePath, FileMode.Open, FileAccess.Read)))
            {
                byte[] meta = nativeReader.ReadBytes(16);
                byte[] end = nativeReader.ReadToEnd();
                if (oldRes.ResType == 3639990959U)
                {
                    ShaderBlockDepot shaderBlockDepot = new ShaderBlockDepot();
                    using (NativeReader reader = new NativeReader((System.IO.Stream)new MemoryStream(end)))
                        shaderBlockDepot.Read(reader, App.AssetManager, oldRes, (ModifiedResource)null);
                    for (int index = 0; index < shaderBlockDepot.ResourceCount; ++index)
                    {
                        ShaderBlockResource resource = shaderBlockDepot.GetResource(index);
                        switch (resource)
                        {
                            case FrostySdk.Resources.ShaderPersistentParamDbBlock _:
                            case FrostySdk.Resources.MeshParamDbBlock _:
                                resource.IsModified = true;
                                break;
                        }
                    }
                    App.AssetManager.ModifyRes(oldRes.Name, (Resource)shaderBlockDepot, meta);
                }
                else
                    App.AssetManager.ModifyRes(oldRes.Name, end, meta);
            }
            App.Logger.Log($"Imported res file: {newRes.fileName}");
            return 0;
        }

        private static void InitResChunkLists(FrostyChunkResExplorer chunkResExplorer)
        {
            ListBox chunksListBox = ReflectionHelper.GetFieldValue<ListBox>(chunkResExplorer, "chunksListBox");

            // Add all chunks to a list 
            _allChunks = new List<ChunkAssetEntry>();
            foreach (ChunkAssetEntry chunk in chunksListBox.Items)
            {
                _allChunks.Add(chunk);
            }

            // Add all res files to a list
            // use dispatcher to access resExplorer element in the UI thread
            _allResFiles = new List<ResAssetEntry>();
            _mainWindow.Dispatcher.Invoke((Action) (()=>
            {
                foreach (AssetEntry res in _resExplorer.ItemsSource)
                {
                    _allResFiles.Add(res as ResAssetEntry);
                }
            }));
        }

        public int ExportResFile()
        {
            ResAssetEntry selectedAsset = null;
            // Use dispatcher to get access to UI element selected asset
            _mainWindow.Dispatcher.Invoke((Action)(() =>
            {
                selectedAsset = _resExplorer.SelectedAsset as ResAssetEntry;
            }));
            if(selectedAsset == null)
            {
                return (int)errorState.NoResFileSelected;
            }

            FrostySaveFileDialog sfd = new FrostySaveFileDialog("Save Resource", "*.res (Resource Files)|*.res", "Res", selectedAsset.Filename, true);
            System.IO.Stream resStream = App.AssetManager.GetRes(selectedAsset);
            if (resStream == null || !sfd.ShowDialog())
            {
                return (int)errorState.ResFileNotFound;
            }
            // Check if file already exists, if so, exit operation
            string selectedFile = sfd.FileName;
            if(File.Exists(selectedFile))
            {
                return (int)errorState.CannotOverwriteExistingFile;
            }

            // Hard encode res id into data.json file
            ChunkResFile curFile;
            string resRid = selectedAsset.ResRid.ToString();
            string parentDir = Path.GetDirectoryName(selectedFile);
            string tempFileName = Path.GetFileName(selectedFile);
            string extension = ".res";
            string justFileName = tempFileName.Substring(0, tempFileName.Length - extension.Length);
            curFile = new ChunkResFile(selectedFile, parentDir, justFileName, extension, resRid);
            string jsonStr = JsonConvert.SerializeObject(curFile);
            // Check if res file has already been exported, if not, add to list and json
            ChunkResFile searchFile;
            if ((searchFile = IsAlreadyInList(resRid)) == null)
            {
                _exportedResFiles.Add(curFile);
                File.AppendAllText(RES_DATA_PATH, "\n" + jsonStr);
            }
            else if(searchFile != null && searchFile.parentDir != curFile.parentDir)  // Check if file location has changed, if so, replace in list, rewrite json
            {
                _exportedResFiles.Remove(searchFile);
                _exportedResFiles.Add(curFile);
                SerializeToJson();
            }

            using (NativeWriter nativeWriter = new NativeWriter(new FileStream(selectedFile, FileMode.Create), false, false))
            {
                nativeWriter.Write(selectedAsset.ResMeta);
                using (NativeReader nativeReader = new NativeReader(resStream))
                    nativeWriter.Write(nativeReader.ReadToEnd());
            }
            App.Logger.Log($"Exported res file: {selectedAsset.Name}");
            return (int)errorState.Success;
        }

        private static byte[] Initialize(byte b, byte[] arr)
        {
            for(int i = 0; i < arr.Length; i++)
            {
                arr[i] = (byte)b;
            }
            return arr;
        }

        private bool CompareAssets(AssetEntry f)
        {
            return f.Name == _searchTerm;
        }

        private bool CompareByRid(ChunkResFile f)
        {
            
            return f.resRid == _searchTerm;
            
        }

        private bool CompareByParentDir(ChunkResFile f)
        {
            return f.parentDir == _searchTerm;
        }

        private ChunkResFile IsAlreadyInList(string resRid)
        {
            Predicate<ChunkResFile> predicate = CompareByRid;
            _searchTerm = resRid;
            ChunkResFile search;
            search = _exportedResFiles.Find(predicate);
            return search;
        }

        private void SerializeToJson()
        {
            File.Delete(RES_DATA_PATH);
            foreach (ChunkResFile f in _exportedResFiles)
            {
                string jsonStr = JsonConvert.SerializeObject(f);
                File.AppendAllText(RES_DATA_PATH, "\n" + jsonStr);
            }
            //File.SetAttributes(dataPath, FileAttributes.Hidden);
        }

        private void DeserializeFromJson()
        {
            _exportedResFiles = new List<ChunkResFile>();
            if (!File.Exists(RES_DATA_PATH))
            {
                return;
            }
            string[] allLines = File.ReadAllLines(RES_DATA_PATH);
            for(int i = 1; i < allLines.Length; i++)
            {
                ChunkResFile f = JsonConvert.DeserializeObject<ChunkResFile>(allLines[i]);
                _exportedResFiles.Add(f);
            }
        }

        // Revert given asset
        public void RevertAsset(AssetEntry asset, bool chunk)
        {
            string assetType = chunk ? "chunk file" : "res file";
            if (asset == null || !asset.IsModified)
                return;
            App.AssetManager.RevertAsset(asset, false, false);
            App.Logger.Log($"Reverted {assetType}: {asset.Name}");
        }
    }
}

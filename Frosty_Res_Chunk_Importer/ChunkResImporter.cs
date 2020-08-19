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
        private FrostyDataExplorer _resExplorer;
        private FrostyChunkResExplorer _chunkResExplorer;
        private static List<ChunkAssetEntry> _allChunks;
        private static List<ResAssetEntry> _allResFiles;
        private static List<ChunkResFile> _exportedResFiles;
        private List<ChunkResFile> _chunkFiles;
        private List<ChunkResFile> _resFiles;
        private string _searchTerm;
        private string _searchTerm2;

        public ChunkResImporter(MainWindow mainWindow, FrostyChunkResExplorer chunkResExplorer, FrostyDataExplorer resExplorer, List<ChunkResFile> chunkFiles, List<ChunkResFile> resFiles)
        {
            _mainWindow = mainWindow;
            _chunkResExplorer = chunkResExplorer;
            _resExplorer = resExplorer;
            _chunkFiles = chunkFiles;
            _resFiles = resFiles;
            
            // Index chunk/res explorer asset entries
            InitResChunkLists(_chunkResExplorer, _resExplorer);
        }

        // Facilitates chunk imports from chunk files list. Takes boolean which denotes whether to import or revert files
        public FrostyDataExplorer Import(bool revert, string parentDir)
        {
            // Checks if chunk and res explorer need to be re-indexed
            if (_allChunks == null || _allResFiles == null)
            {
                InitResChunkLists(_chunkResExplorer, _resExplorer);
            }
            // Instantiate predicate delegates
            Predicate<AssetEntry> namePredicate = CompareAssets;
            Predicate<ChunkResFile> absPathPredicate = CompareByAbsolutePath;
            Predicate<ResAssetEntry> resRidPredicate = CompareByRid;

            // Find and import chunks in Frosty chunk explorer
            foreach (ChunkResFile newChunk in _chunkFiles)
            {
                _searchTerm = newChunk.fileName;
                ChunkAssetEntry oldChunk = _allChunks.Find(namePredicate);
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
                // Check if res file is documented in exported res files list, if not, log warning and exit operation
                _searchTerm = newRes.absolutePath;
                ChunkResFile intermediate = _exportedResFiles.Find(absPathPredicate);
                if(intermediate == null)
                {
                    App.Logger.Log($"WARNING: Res file located at {newRes.absolutePath} must be imported manually. Unable to locate res file data. ");
                    return _resExplorer;
                }
                _searchTerm = intermediate.resRid;
                ResAssetEntry oldRes = _allResFiles.Find(resRidPredicate);
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
            return (int)errorState.Success;
        }

        private static void InitResChunkLists(FrostyChunkResExplorer chunkResExplorer, FrostyDataExplorer resExplorer)
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
                foreach (AssetEntry res in resExplorer.ItemsSource)
                {
                    _allResFiles.Add(res as ResAssetEntry);
                }
            }));
        }

        public int ExportResFile(ResAssetEntry selectedAsset, string selectedFile)
        {
            // Hard encode res id into exported res file list
            ChunkResFile curFile;
            string resRid = selectedAsset.ResRid.ToString();
            string parentDir = Path.GetDirectoryName(selectedFile);
            string tempFileName = Path.GetFileName(selectedFile);
            string extension = ".res";
            string justFileName = tempFileName.Substring(0, tempFileName.Length - extension.Length);
            curFile = new ChunkResFile(selectedFile, parentDir, justFileName, extension, resRid);

            // Check if res file has already been exported, if not, add to list
            ChunkResFile searchFile;
            if ((searchFile = IsAlreadyInList(resRid)) == null)
            {
                _exportedResFiles.Add(curFile);
            }
            else if(searchFile != null && searchFile.parentDir != curFile.parentDir)  // Check if file location has changed, if so, replace in list, rewrite json
            {
                _exportedResFiles.Remove(searchFile);
                _exportedResFiles.Add(curFile);
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

        private bool CompareAssets(AssetEntry f)
        {
            return f.Name == _searchTerm;
        }

        private bool CompareByRid(ResAssetEntry f)
        {
            
            return f.ResRid.ToString() == _searchTerm;
            
        }

        private bool CompareByRidGeneric(ChunkResFile f)
        {
            return f.resRid == _searchTerm;
        }

        private bool CompareByAbsolutePath(ChunkResFile f)
        {
            return f.absolutePath == _searchTerm;
        }

        private ChunkResFile IsAlreadyInList(string resRid)
        {
            Predicate<ChunkResFile> predicate = CompareByRidGeneric;
            _searchTerm = resRid;
            ChunkResFile search;
            search = _exportedResFiles.Find(predicate);
            return search;
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

// ChunkResImporter.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System.IO;

// <summary>
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
            if(_exportedResFiles == null)
            {
                _exportedResFiles = new List<ChunkResFile>();
            }
            // Checks if chunk and res explorer need to be re-indexed
            if (_allChunks == null || _allResFiles == null)
            {
                InitResChunkLists(_chunkResExplorer, _resExplorer);
            }
        }

        // Facilitates chunk imports from chunk files list. Takes boolean which denotes whether to import or revert files
        public int Import(bool revert, string parentDir)
        {
            // Instantiate predicate delegates
            Predicate<AssetEntry> namePredicate = CompareAssets;
            Predicate<ChunkResFile> dirPredicate = CompareByDir;
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

            string operation = revert ? "reverted" : "imported";
            // Find and import res files in Frosty res explorer
            foreach (ChunkResFile newRes in _resFiles)
            {
                // Check if res file is documented in exported res files list, if not, log warning and exit operation
                _searchTerm = newRes.directory;
                _searchTerm2 = newRes.fileName;
                ChunkResFile intermediate = _exportedResFiles.Find(dirPredicate);
                if(intermediate == null)
                {
                    App.Logger.Log($"WARNING: {errorState.NonCriticalResFileError}: {errorState.MissingResID}. Res file located at {newRes.absolutePath} must be {operation} manually. Unable to locate res file data.");
                } 
                else
                {
                    _searchTerm = intermediate.resRid;
                    ResAssetEntry oldRes = _allResFiles.Find(resRidPredicate);
                    // Check if res file found, if not log and exit
                    if (oldRes == null)
                    {
                        return (int)errorState.CriticalResFileError;
                    }
                    if (revert)
                    {
                        RevertAsset(oldRes, false);
                    }
                    else
                    {
                        ImportResFiles(oldRes, newRes);
                    }
                }
            }
            return (int)errorState.Success;
        }

        // Imports a single chunk. Takes chunk to replace and path to new chunk
        private int ImportChunk(ChunkAssetEntry oldChunk, ChunkResFile newChunk)
        {        
            using (NativeReader nativeReader = new NativeReader(new FileStream(newChunk.absolutePath, FileMode.Open, FileAccess.Read)))
            {
                byte[] end = nativeReader.ReadToEnd();
                App.AssetManager.ModifyChunk(oldChunk.Id, end, (Texture)null);
            }
            App.Logger.Log($"Imported chunk file: {oldChunk.Name}");
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

        public int ExportResFile(ResAssetEntry selectedAsset, string selectedFile)
        {
            // Hard encode res id into exported res file list
            ChunkResFile curFile;
            string resRid = selectedAsset.ResRid.ToString();
            curFile = new ChunkResFile(selectedFile, resRid);
            
            // Check if res file has already been exported, if not, add to list
            ChunkResFile searchFile;
            if ((searchFile = IsAlreadyInList(resRid)) == null)
            {
                _exportedResFiles.Add(curFile);
            }
            else if(searchFile != null && searchFile.absolutePath != curFile.absolutePath)  // Check if file location has changed, if so, replace in list, rewrite json
            {
                _exportedResFiles.Remove(searchFile);
                _exportedResFiles.Add(curFile);
            }
            Stream resStream = App.AssetManager.GetRes(selectedAsset);
            if(resStream == null)
            {
                return (int)errorState.CriticalResFileError;
            }
            using (NativeWriter nativeWriter = new NativeWriter(new FileStream(selectedFile, FileMode.Create), false, false))
            {
                nativeWriter.Write(selectedAsset.ResMeta);
                using (NativeReader nativeReader = new NativeReader(resStream))
                    nativeWriter.Write(nativeReader.ReadToEnd());
            }
            App.Logger.Log($"Exporting res file: {selectedAsset.Name}");
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
            _mainWindow.Dispatcher.Invoke((Action)(() =>
            {
                foreach (AssetEntry res in resExplorer.ItemsSource)
                {
                    _allResFiles.Add(res as ResAssetEntry);
                }
            }));
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

        private bool CompareByDir(ChunkResFile f)
        {
            return f.directory == _searchTerm && f.fileName == _searchTerm2;
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


    // JSON Serialization methods

    //private void SerializeToJson()
    //{
    //    File.Delete(RES_DATA_PATH);
    //    foreach (ChunkResFile f in _exportedResFiles)
    //    {
    //        string jsonStr = JsonConvert.SerializeObject(f);
    //        File.AppendAllText(RES_DATA_PATH, "\n" + jsonStr);
    //    }
    //    //File.SetAttributes(dataPath, FileAttributes.Hidden);
    //}

    //private void DeserializeFromJson()
    //{
    //    _exportedResFiles = new List<ChunkResFile>();
    //    if (!File.Exists(RES_DATA_PATH))
    //    {
    //        return;
    //    }
    //    string[] allLines = File.ReadAllLines(RES_DATA_PATH);
    //    for (int i = 1; i < allLines.Length; i++)
    //    {
    //        ChunkResFile f = JsonConvert.DeserializeObject<ChunkResFile>(allLines[i]);
    //        _exportedResFiles.Add(f);
    //    }
    //}
}

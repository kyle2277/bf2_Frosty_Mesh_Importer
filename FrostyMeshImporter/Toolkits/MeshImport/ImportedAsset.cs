// ImportedAsset.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostyMeshImporter.Toolkits.MeshImport
{
    // TODO: consider combining ImportedAsset and MeshSet struct
    //Imported asset structure used for reversion of imported meshes
    public class ImportedAsset
    {
        public string meshSetName { get; }
        public string directory { get; set; }
        public bool canImportRes { get; set; }
        public List<ChunkResFile> chunks { get; }
        public List<ChunkResFile> res { get; }
        public ImportedAsset(string meshSetName, string directory, List<ChunkResFile> chunks, List<ChunkResFile> res)
        {
            this.meshSetName = meshSetName;
            this.directory = directory;
            this.chunks = chunks;
            this.res = res;
            canImportRes = false;
        }

        public override string ToString()
        {
            return this.meshSetName;
        }
    }
}

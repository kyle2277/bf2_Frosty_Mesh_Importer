// ImportedAsset.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostyResChunkImporter
{
    //Imported asset structure used for reversion of imported meshes
    public class ImportedAsset
    {
        public string meshSetName { get; }
        public string directory { get; set; }
        public List<ChunkResFile> chunks { get; }
        public List<ChunkResFile> res { get; }
        public ImportedAsset(string meshSetName, string directory, List<ChunkResFile> chunks, List<ChunkResFile> res)
        {
            this.meshSetName = meshSetName;
            this.directory = directory;
            this.chunks = chunks;
            this.res = res;
        }

        public override string ToString()
        {
            return this.meshSetName;
        }
    }
}

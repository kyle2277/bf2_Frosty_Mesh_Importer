// ChunkResFile.cs - FrostyResChunkImporter
// Contributors:
//      Copyright (C) 2020  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FrostyMeshImporter
{
    public class ChunkResFile
    {
        public string absolutePath { get; }
        public string meshSetName { get; }
        public string fileName { get; }
        public string extension { get; }
        public string resRid { get; set; }

        public ChunkResFile(string absolutePath, string resRid)
        {
            this.absolutePath = absolutePath;
            this.resRid = resRid;

            extension = Path.GetExtension(absolutePath);
            string tempFileName = Path.GetFileName(absolutePath);
            string[] pathSplit = Path.GetDirectoryName(absolutePath).Split('\\');
            meshSetName = pathSplit[pathSplit.Length - 1];
            // Remove extension from file name
            fileName = tempFileName.Substring(0, tempFileName.Length - extension.Length);
        }
    }
}

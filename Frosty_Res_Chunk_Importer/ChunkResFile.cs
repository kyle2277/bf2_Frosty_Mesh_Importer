using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostyResChunkImporter
{
    class ChunkResFile
    {
        public string absolutePath { get; set; }
        public string parentDir { get; set; }
        public string fileName { get; }
        public string extension { get; }
        public string resRid { get; set; }

        public ChunkResFile(string absolutePath, string parentDir, string fileName, string extension, string resRid)
        {
            this.absolutePath = absolutePath;
            this.parentDir = parentDir;
            this.fileName = fileName;
            this.extension = extension;
            this.resRid = resRid;
        }
    }
}

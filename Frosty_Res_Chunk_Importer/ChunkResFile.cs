using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FrostyResChunkImporter
{
    class ChunkResFile
    {
        public string absolutePath { get; }
        public string directory { get; }
        public string fileName { get; }
        public string extension { get; }
        public string resRid { get; }

        public ChunkResFile(string absolutePath, string resRid)
        {
            this.absolutePath = absolutePath;
            this.resRid = resRid;

            extension = Path.GetExtension(absolutePath);
            string tempFileName = Path.GetFileName(absolutePath);
            string[] pathSplit = Path.GetDirectoryName(absolutePath).Split('\\');
            // Set directory to last item in string split array
            directory = pathSplit[pathSplit.Length -1 ];
            // Remove extension from file name
            fileName = tempFileName.Substring(0, tempFileName.Length - extension.Length);
        }
    }
}

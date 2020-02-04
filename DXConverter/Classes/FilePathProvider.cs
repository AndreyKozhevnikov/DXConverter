using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
    interface IFilePathProvider {
        string GetFilePath(string fileName);
    }

    public class SimpleFilePathProvider : IFilePathProvider {
        public string SourceFolderPath { get;set; }
        public string Version { get;set; }

        public string GetFilePath(string fileName) {
            return Path.Combine(SourceFolderPath, Version, fileName);
        }
    }
}

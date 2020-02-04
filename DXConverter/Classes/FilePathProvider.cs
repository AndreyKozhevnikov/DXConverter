using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
    public interface IFilePathProvider {
        string GetFilePath(string fileName);
        string GetDllDirectory();
        string Version { get; set; }
    }

    public class SimpleFilePathProvider : IFilePathProvider {
        public SimpleFilePathProvider(string _sourcePath, string _version,string _projectFolder) {
            SourceFolderPath = _sourcePath;
            Version = _version;
            ProjectFolder = _projectFolder;
        }

        public string SourceFolderPath { get; set; }
        public string Version { get; set; }

        public string ProjectFolder{ get; set; }

        public string GetDllDirectory() {
            return Path.Combine(ProjectFolder, "DLL");
        }

        public string GetFilePath(string fileName) {
            return Path.Combine(SourceFolderPath, Version, fileName);
        }

    }
}

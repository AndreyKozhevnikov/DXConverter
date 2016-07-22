using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DXConverter {
    class Program {
        static void Main(string[] args) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesObject = new CustomFileDirectoriesClass();
            a.ProjectConverterProcessorObject = new ProjectConverterProcessor();
            //  string projPath = @"f:\Dropbox\C#\temp\DXConverter\dxSampleGrid\"; 
            string projPath = @"c:\Dropbox\C#\temp\DXConverter\dxSampleGrid\";
            a.ProcessProject(projPath, "15.2.5");
            Console.WriteLine("end");
            Console.Read();
        }
    }


    public class AssemblyConverter {
        public ICustomFileDirectories CustomFileDirectoriesObject;
        public IProjectConverterProcessor ProjectConverterProcessorObject;
        public const string defaultPath = @"\\CORP\builds\release\DXDlls\";

        public List<string> GetVersions() {
            List<string> directories = new List<string>();
            try {
                var allDirectories = CustomFileDirectoriesObject.GetDirectories(defaultPath);
                foreach (string directory in allDirectories)
                    directories.Add(Path.GetFileName(directory));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            directories.Sort(new VersionComparer());
            return directories;
        }

        internal void ProcessProject(string projectFolder, string version) {
            var converterPath = GetProjectConverterPath(defaultPath, version);
            ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
            var projFiles = GetProjFiles(projectFolder, new string[] { "*.csproj", "*.vbproj" });
            foreach (string projPath in projFiles) {
                CopyAssembliesToProj(projPath, defaultPath, version);
            }

        }

        private void CopyAssembliesToProj(string projectPath, string sourcePath, string targetVersion) {
            XDocument projDocument = XDocument.Load(projectPath);

            List<XElement> xlLibraries = GetLibrariesXL(projDocument);
            List<string> stLibraries = GetLibrariesString(xlLibraries);
            string libraryDirectory = Path.Combine(sourcePath, targetVersion);
            List<LibraryFileInfo> stLibrariesPaths = GetLibrariesPath(stLibraries, libraryDirectory);
            string directoryDestination = GetDirectoryDesctination(projectPath);
            CreateDirectoryDestinationIfNeeded(directoryDestination);
            foreach (LibraryFileInfo libFileInfo in stLibrariesPaths) {
                CopyAssemblyCore(directoryDestination, libFileInfo);
            }
        }

        public void CreateDirectoryDestinationIfNeeded(string directoryDestination) {
            if (!CustomFileDirectoriesObject.IsDirectoryExist(directoryDestination)) {
                CustomFileDirectoriesObject.CreateDirectory(directoryDestination);
            }
        }






        public List<XElement> GetLibrariesXL(XDocument projDocument) {
            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            var lst = projDocument
                                      .Element(msbuild + "Project")
                                      .Elements(msbuild + "ItemGroup")
                                      .Elements(msbuild + "Reference")
                                      .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress"))
                                      .ToList();
            return lst;
        }
        public List<string> GetLibrariesString(List<XElement> xlLibraries) {
            var st = xlLibraries.Select(x => x.FirstAttribute.Value.Split(',')[0]).ToList();
            return st;
        }

        public List<LibraryFileInfo> GetLibrariesPath(List<string> stLibraries, string libraryDirectory) {
            List<LibraryFileInfo> lst = new List<LibraryFileInfo>();
            foreach (string assembly in stLibraries) {
                string assemblyName = assembly + ".dll";
                string assemblyPath = Path.Combine(libraryDirectory, assemblyName);
                LibraryFileInfo li = new LibraryFileInfo(assemblyName, assemblyPath);
                if (CustomFileDirectoriesObject.IsFileExist(assemblyPath)) {
                    lst.Add(li);
                }
            }
            return lst;
        }
        public List<string> GetProjFiles(string applicationPath, string[] extenshions) {
            List<string> projFiles = new List<string>();
            foreach (string extenshion in extenshions) {
                //   projFiles.AddRange(Directory.GetFiles(applicationPath, exteshion, SearchOption.AllDirectories));
                projFiles.AddRange(CustomFileDirectoriesObject.GetFiles(applicationPath, extenshion));
            }
            return projFiles;
        }

        public string GetProjectConverterPath(string sourcePath, string targetVersion) {
            string projectConverterConsolePath = Path.Combine(sourcePath, targetVersion, "ProjectConverter-console.exe");
            return projectConverterConsolePath;
        }
        public void CopyAssemblyCore(string directoryDestination, LibraryFileInfo libFileInfo) {
            string fileDesctination = Path.Combine(directoryDestination, libFileInfo.FileName);
            string fileSource = libFileInfo.FileNameWithPath;
            CustomFileDirectoriesObject.FileCopy(fileSource, fileDesctination, true);
        }
        public string GetDirectoryDesctination(string projectPath) {
            string st = Path.Combine(Directory.GetParent(projectPath).FullName, @"bin\Debug");
            return st;
        }
    }

    public class VersionComparer : IComparer<string> {

        public int Compare(string x, string y) {
            int counter = 0, res = 0;
            while (counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter]);
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }

    public interface ICustomFileDirectories {
        string[] GetDirectories(string path);
        string[] GetFiles(string path, string pattern);
        bool IsFileExist(string path);
        bool IsDirectoryExist(string path);
        DirectoryInfo CreateDirectory(string path);
        void FileCopy(string source, string desctination, bool overwrite);
    }
    public class CustomFileDirectoriesClass : ICustomFileDirectories {

        public string[] GetDirectories(string path) {
            return Directory.GetDirectories(path);
        }
        public string[] GetFiles(string path, string pattern) {
            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
        }

        public bool IsFileExist(string path) {
            return File.Exists(path);
        }


        public bool IsDirectoryExist(string path) {
            return Directory.Exists(path);
        }


        public DirectoryInfo CreateDirectory(string path) {
            return Directory.CreateDirectory(path);
        }


        public void FileCopy(string source, string desctination, bool overwrite) {
            File.Copy(source, desctination, overwrite);
        }
    }

    public interface IProjectConverterProcessor {
        void Convert(string converterPath, string projectFolder);
    }
    public class ProjectConverterProcessor : IProjectConverterProcessor {
        public void Convert(string converterPath, string projectFolder) {
            ProcessStartInfo startInfo = new ProcessStartInfo(converterPath, "\"" + projectFolder + "\"");
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            using (Process process = new Process()) {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }
    }
    public class LibraryFileInfo {
        public LibraryFileInfo(string _fileName, string _fileNameWithPath) {
            FileName = _fileName;
            FileNameWithPath = _fileNameWithPath;
        }
        public string FileName;
        public string FileNameWithPath;
    }
}

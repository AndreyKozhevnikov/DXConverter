using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
    class Program {
        static void Main(string[] args) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesFactory = new CustomFileDirectoriesClass();
            var l = a.GetVersions();
            a.ProcessProject(@"c:\Dropbox\C#\temp\DXConverter\dxSampleGrid\", "15.2.5");
            Console.Read();
        }
    }


    public class AssemblyConverter {
        public ICustomFileDirectories CustomFileDirectoriesFactory;

        public const string defaultPath = @"\\CORP\builds\release\DXDlls\";

        public List<string> GetVersions() {
            List<string> directories = new List<string>();
            try {
                var allDirectories = CustomFileDirectoriesFactory.GetDirectories(defaultPath);
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
            var listVersions = GetVersions();
            if (!listVersions.Contains(version))
                return;
            var converterPath = GetProjectConverterPath(defaultPath, version);
            if (converterPath == null)
                return;
          



         

        }

        public string GetProjectConverterPath(string sourcePath, string targetVersion) {
            string projectConverterConsolePath = Path.Combine(sourcePath, targetVersion, "ProjectConverter-console.exe");
            //  string projectConverterPath = Path.Combine(sourcePath, targetVersion, "ProjectConverter.exe");
            if (CustomFileDirectoriesFactory.IsFileExist(projectConverterConsolePath))
                return projectConverterConsolePath;
            return null;
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

  public  interface ICustomFileDirectories {
        string[] GetDirectories(string path);
        bool IsFileExist(string path);
    }
    public class CustomFileDirectoriesClass : ICustomFileDirectories {

        public string[] GetDirectories(string path) {
            return Directory.GetDirectories(path);
        }


        public bool IsFileExist(string path) {
            return File.Exists(path);
        }
    }
}

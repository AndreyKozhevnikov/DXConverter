﻿using System;
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
            a.ProcessProject(@"c:\Dropbox\C#\temp\DXConverter\dxSampleGrid\", "15.2.5");
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

            List<XElement> xlLibraries = GetDevExpressXLElements(projDocument);
            List<string> stLibraries = GetLibraries(xlLibraries);
            //List<string> assemblies = GetAssembliesFromProj(projectPath,
        }

        public List<string> GetLibraries(List<XElement> xlLibraries) {
            throw new NotImplementedException();
        }

        public List<XElement> GetDevExpressXLElements(XDocument projDocument) {
            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            var lst= projDocument
                                      .Element(msbuild + "Project")
                                      .Elements(msbuild + "ItemGroup")
                                      .Elements(msbuild + "Reference")
                                      .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress"))
                                      .ToList();
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
    }
    public class CustomFileDirectoriesClass : ICustomFileDirectories {

        public string[] GetDirectories(string path) {
            return Directory.GetDirectories(path);
        }
        public string[] GetFiles(string path, string pattern) {
            return Directory.GetDirectories(path, pattern, SearchOption.AllDirectories);
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
}

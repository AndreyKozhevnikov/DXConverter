using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DXConverter {
    class Program {
        static void Main(string[] args) {
            var cnt = args.Count();
            switch (cnt) {
                case 2:
                    ConvertProject(args);
                    Console.Read();
                    break;
                case 3:
                    bool waitForExit = bool.Parse(args[2]);
                    ConvertProject(args);
                    if (waitForExit) {
                        Console.Read();
                    }
                    break;
                default:
                    Console.WriteLine("Wrong arguments");
                    Console.WriteLine(string.Join("\r\n", args));
                    Console.Read();
                    break;

            }
            //if (args.Count() == 2) {
            //    ConvertProject(args);
            //    Console.Read();
            //}
            //else {
            //    Console.WriteLine("Wrong arguments");
            //    Console.WriteLine(string.Join("\r\n", args));
            //    Console.Read();
            //}
        }

        private static void ConvertProject(string[] args) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesObject = new CustomFileDirectoriesClass();
            a.ProjectConverterProcessorObject = new ProjectConverterProcessor();
            a.MessageProcessor = new ConsoleMessageProcessor();
            var projPath = args[0];
            var vers = args[1];
            a.MyWorkWithFile = new CustomWorkWithFile();
            a.ProcessProject(projPath, vers);
            Console.WriteLine("end");
        }
    }


    public class AssemblyConverter {
        public ICustomFileDirectories CustomFileDirectoriesObject;
        public IProjectConverterProcessor ProjectConverterProcessorObject;
        public IMessageProcessor MessageProcessor;
        public const string defaultPath = @"\\CORP\builds\release\DXDlls\";
        public const string debugPath = @"bin\Debug\";
        public static XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
        public IWorkWithFile MyWorkWithFile;
        //public List<string> GetVersions() {
        //    List<string> directories = new List<string>();
        //    try {
        //        var allDirectories = CustomFileDirectoriesObject.GetDirectories(defaultPath);
        //        directories = allDirectories.Select(x => Path.GetFileName(x)).ToList();
        //    }
        //    catch (Exception e) {
        //        Console.WriteLine(e.Message);
        //    }
        //    directories.Sort(new VersionComparer());
        //    return directories;
        //}

        internal void ProcessProject(string projectFolder, string version) {
            MessageProcessor.SendMessage("Start");

            var installedVersions = GetInstalledVersions();
            bool isVersionInstalled = installedVersions.ContainsKey(version);
            if (isVersionInstalled) {
                MessageProcessor.SendMessage("Convert to installed version");
                var converterPath = installedVersions[version];
                ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                MessageProcessor.SendMessage("Project converter complete");
            }
            else {
                var converterPath = Path.Combine(defaultPath, version, "ProjectConverter-console.exe");
                ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                MessageProcessor.SendMessage("Project converter complete");
                var projFiles = GetProjFiles(projectFolder, new string[] { "*.csproj", "*.vbproj" });
                foreach (string projPath in projFiles) {
                    ProcessCSProjFile(projPath, defaultPath, version);
                }
            }
            MessageProcessor.SendMessage("Finish");
        }

        public void ProcessCSProjFile(string projectPath, string sourcePath, string targetVersion) {
            XDocument projDocument = CustomFileDirectoriesObject.LoadXDocument(projectPath);
            string libraryDirectory = Path.Combine(sourcePath, targetVersion);
            // List<LibraryInfo> librariesList = GetFullLibrariesInfo(projDocument, libraryDirectory);
            List<XElement> xlLibraries = GetLibrariesXL(projDocument);
            var isVersion16 = int.Parse(targetVersion.Split('.')[0].ToString()) >= 16;
            if (isVersion16) {
                var isOffThemeExist = xlLibraries.Where(x => x.FirstAttribute.Value.Contains("Office2016White")).Count() > 0;
                if (!isOffThemeExist) {
                    AddOfficeThemeToDocument(projDocument, xlLibraries);

                }
            }
            string directoryDestination = GetDirectoryDesctination(projectPath);
            CreateDirectoryDestinationIfNeeded(directoryDestination);
            var libFileName = Path.Combine(directoryDestination, "dxLibraries.txt");
            Dictionary<string, string> existingLibrariesDictionary = GetExistingLibraries(libFileName);
            List<LibraryInfo> librariesList = new List<LibraryInfo>();
            foreach (XElement xl in xlLibraries) {
                string fileName = xl.FirstAttribute.Value.Split(',')[0];
                string assemblyName = fileName + ".dll";

                LibraryInfo libFileInfo = new LibraryInfo();
                libFileInfo.FileName = assemblyName;
                libFileInfo.XMLelement = xl;
                librariesList.Add(libFileInfo);

                ChangeHintPath(libFileInfo);
                RemoveSpecVersion(libFileInfo);
                bool isLibraryAlreadyExist = CheckIfLibraryAlreadyExist(libFileInfo, existingLibrariesDictionary, targetVersion);

                //if (!isLibraryAlreadyExist&&isFileExist) {
                if (!isLibraryAlreadyExist) {
                    string assemblyPath = Path.Combine(libraryDirectory, assemblyName);
                    bool isFileExist = CustomFileDirectoriesObject.IsFileExist(assemblyPath);
                    if (isFileExist) {
                        libFileInfo.FileNameWithPath = assemblyPath;
                        CopyAssemblyCore(directoryDestination, libFileInfo);
                        MessageProcessor.SendMessage(libFileInfo.FileName + " Copied");
                    }
                    else {
                        MessageProcessor.SendMessage(libFileInfo.FileName + " Wrong library");
                    }

                }
                else {
                    MessageProcessor.SendMessage(libFileInfo.FileName + " Skipped");
                }
            }
            var libListForFile = GetStringFromLibrariesList(librariesList, targetVersion);
            CustomFileDirectoriesObject.WriteTextInFile(libFileName, libListForFile);
            CustomFileDirectoriesObject.SaveXDocument(projDocument, projectPath);

        }


        Dictionary<string, string> GetInstalledVersions() {//5 td

            var installedVersions = new Dictionary<string, string>();
            List<string> versions = MyWorkWithFile.GetRegistryVersions("SOFTWARE\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter-console.exe";
            foreach (string rootPath in versions) {
                var rootPath2 = Path.Combine(rootPath, projectUpgradeToolRelativePath);
                string libVersion = GetProjectUpgradeVersion(rootPath2);
                installedVersions[libVersion] = rootPath2;
            }
            return installedVersions;
        }
        string GetProjectUpgradeVersion(string projectUpgradeToolPath) {//5.1 td
            string assemblyFullName = MyWorkWithFile.AssemblyLoadFileFullName(projectUpgradeToolPath);
            string versionAssemblypattern = @"version=(?<Version>\d+\.\d.\d+)";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatch = regexVersion.Match(assemblyFullName);
            string versValue = versionMatch.Groups["Version"].Value;
            return versValue;
        }
        public bool CheckIfLibraryAlreadyExist(LibraryInfo _libFileInfo, Dictionary<string, string> _existingLibrariesDictionary, string _targetVersion) {
            if (_existingLibrariesDictionary.ContainsKey(_libFileInfo.FileName)) {
                return _existingLibrariesDictionary[_libFileInfo.FileName] == _targetVersion;
            }
            return false;
        }

        public Dictionary<string, string> GetExistingLibraries(string filePath) {
            var st = CustomFileDirectoriesObject.GetStringFromFile(filePath);
            if (st != null) {
                var d = ParseStringToDictionary(st);
                return d;
            }
            return new Dictionary<string, string>();
        }

        public Dictionary<string, string> ParseStringToDictionary(string _string) {
            var list1 = _string.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string st in list1) {
                var list2 = st.Split(' ');
                dict[list2[0]] = list2[1];
            }
            return dict;
        }

        public string GetStringFromLibrariesList(List<LibraryInfo> list, string targetVersion) {
            var libListForFile = string.Join(Environment.NewLine, list.Select(x => x.FileName + " " + targetVersion));
            return libListForFile;
        }


        public void ChangeHintPath(LibraryInfo libFileInfo) {
            XElement elem = libFileInfo.XMLelement;
            XName hintPath = msbuild + "HintPath";
            XElement hintPathElem = elem.Element(hintPath);
            string path = debugPath + libFileInfo.FileName;

            if (hintPathElem == null) {
                hintPathElem = new XElement(hintPath, path);
                elem.Add(hintPathElem);
            }
            else
                hintPathElem.SetValue(path);
        }
        void RemoveSpecVersion(LibraryInfo libraryInfo) {
            XElement elem = libraryInfo.XMLelement;
            var specVersion = elem.Element(AssemblyConverter.msbuild + "SpecificVersion");
            if (specVersion != null)
                specVersion.Remove();
        }


        public List<XElement> GetLibrariesXL(XDocument projDocument) {
            var lst = projDocument
                                      .Element(msbuild + "Project")
                                      .Elements(msbuild + "ItemGroup")
                                      .Elements(msbuild + "Reference")
                                      .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress"))
                                      .ToList();
            return lst;
        }
        private void AddOfficeThemeToDocument(XDocument projDocument, List<XElement> xllist) {
            var gr = projDocument.Element(msbuild + "Project").Elements(msbuild + "ItemGroup").FirstOrDefault();
            var it = new XElement(gr.Elements().First());
            var at = it.Attribute("Include");
            var val = at.Value;
            var dxValue = val.Split(',')[0];
            var newVal = val.Replace(dxValue, "DevExpress.Xpf.Themes.Office2016White.v16.1");
            at.Value = newVal;
            gr.Add(it);
            xllist.Add(it);
        }


        public void CreateDirectoryDestinationIfNeeded(string directoryDestination) {
            if (!CustomFileDirectoriesObject.IsDirectoryExist(directoryDestination)) {
                CustomFileDirectoriesObject.CreateDirectory(directoryDestination);
            }
        }

        public List<string> GetProjFiles(string applicationPath, string[] extenshions) {
            List<string> projFiles = new List<string>();
            foreach (string extenshion in extenshions) {
                projFiles.AddRange(CustomFileDirectoriesObject.GetFiles(applicationPath, extenshion));
            }
            return projFiles;
        }


        public void CopyAssemblyCore(string directoryDestination, LibraryInfo libFileInfo) {
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
    public class LibraryInfo {
        public string FileName;
        public string FileNameWithPath;
        public XElement XMLelement;
    }

    public interface IMessageProcessor {
        void SendMessage(string message);
    }
    public class ConsoleMessageProcessor : IMessageProcessor {
        public ConsoleMessageProcessor() {
            tmpDT = DateTime.Now;
        }
        DateTime tmpDT;
        public void SendMessage(string message) {
            var dt = DateTime.Now - tmpDT;
            message = string.Format("{0} {1}", message, dt.ToString(@"ss\:fff"));
            Console.WriteLine(message);
        }
    }
    public interface IWorkWithFile {
        List<string> GetRegistryVersions(string path);
        string AssemblyLoadFileFullName(string path);
    }

    public class CustomWorkWithFile : IWorkWithFile {
        public List<string> GetRegistryVersions(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var lst = regKey.GetSubKeyNames();
            List<string> resList = new List<string>();
            foreach (string st in lst) {
                RegistryKey dxVersionKey = regKey.OpenSubKey(st);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                resList.Add(projectUpgradeToolPath);
            }
            return resList;
        }
        public string AssemblyLoadFileFullName(string path) {
            try {
                var assembly = Assembly.LoadFile(path);
                return assembly.FullName;
            }
            catch {
                return null;
            }
        }
    }


}

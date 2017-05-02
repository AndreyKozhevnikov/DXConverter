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
            if (cnt == 4 || cnt == 5) {
                bool waitForExit = bool.Parse(args[2]);

                if (waitForExit) {
                    Console.Read();
                }
                string installedPath = null;
                if (cnt == 5) {
                    installedPath = args[4];
                }
                ConvertProject(args[0], args[1], args[3], installedPath);

            }
            else {
                Console.WriteLine("Wrong arguments");
                Console.WriteLine(string.Join("\r\n", args));
                Console.Read();
            }
        }

        private static void ConvertProject(string projPath, string vers, string oldVers, string installedPath) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesObject = new CustomFileDirectoriesClass();
            a.ProjectConverterProcessorObject = new ProjectConverterProcessor();
            a.MessageProcessor = new ConsoleMessageProcessor();

            a.MyWorkWithFile = new CustomWorkWithFile();

            a.ProcessProject(projPath, vers, oldVers, installedPath);

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

        internal void ProcessProject(string projectFolder, string version, string oldVersion, string installedPath) {
            MessageProcessor.SendMessage("Start");
            bool isSameMajor = false;
            if (oldVersion != null) {
                var versMajor = version.Substring(0, 4);
                var oldVersMajor = oldVersion.Substring(0, 4);
                if (versMajor == oldVersMajor) {
                    isSameMajor = true;
                }
            }
            bool isVersionInstalled;
            Dictionary<string, string> installedVersions;
            string converterPath = null;
            if (!string.IsNullOrEmpty(installedPath)) {
                isVersionInstalled = true;
                converterPath = installedPath;
            }
            else {
                installedVersions = GetInstalledVersions();
                isVersionInstalled = installedVersions.ContainsKey(version);
                if (isVersionInstalled)
                    converterPath = installedVersions[version];
            }



            if (isSameMajor) {
                var projFiles = GetProjFiles(projectFolder, new string[] { "*.csproj", "*.vbproj" });
                foreach (string projPath in projFiles) {
                    ProcessCSProjFile(projPath, defaultPath, version, isSameMajor, isVersionInstalled);
                }
            }
            else {
                if (isVersionInstalled) {
                    MessageProcessor.SendMessage("Convert to installed version");
                    ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                    MessageProcessor.SendMessage("Project converter complete");
                }
                else {
                    converterPath = Path.Combine(defaultPath, version, "ProjectConverter-console.exe");
                    ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                    MessageProcessor.SendMessage("Project converter complete");
                    var projFiles = GetProjFiles(projectFolder, new string[] { "*.csproj", "*.vbproj" });
                    foreach (string projPath in projFiles) {
                        ProcessCSProjFile(projPath, defaultPath, version, isSameMajor);
                    }
                }
            }
            MessageProcessor.SendMessage("Finish");
        }

        bool IsLibraryExist(string name, List<XElement> libraries) {
            var ind = name.IndexOf("v00.0");
            var searchString = name.Substring(0, ind);
            return libraries.Where(x => x.FirstAttribute.Value.Contains(searchString)).Count() > 0;
        }

        void AddLibraryIfNotExist(string st, List<XElement> libraries, XDocument projDocument) {
            var b = IsLibraryExist(st, libraries);
            if (!b)
                AddLibraryToDocument(projDocument, libraries, st);

        }

        public void ProcessCSProjFile(string projectPath, string sourcePath, string targetVersion, bool isSameMajor = false, bool isVersionInstalled = false) {
            XDocument projDocument = CustomFileDirectoriesObject.LoadXDocument(projectPath);
            string libraryDirectory = Path.Combine(sourcePath, targetVersion);
            List<XElement> xlLibraries = GetLibrariesXL(projDocument);

            var requiredLibraries = new List<string>();
            requiredLibraries.Add("DevExpress.Data.v00.0");
            requiredLibraries.Add("DevExpress.Printing.v00.0.Core");
            var isVersion16 = int.Parse(targetVersion.Split('.')[0].ToString()) >= 16;
            if (isVersion16) {
                requiredLibraries.Add("DevExpress.Xpf.Themes.Office2016White.v00.0");
            }
            foreach (string st in requiredLibraries) {
                AddLibraryIfNotExist(st, xlLibraries, projDocument);
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
                if (isSameMajor)
                    SetVersion(libFileInfo, targetVersion);
                if (isVersionInstalled)
                    continue;
                ChangeHintPath(libFileInfo);
                RemoveSpecVersion(libFileInfo);
                SetCopyLocalFalse(libFileInfo);
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
            if (!isVersionInstalled) {
                var libListForFile = GetStringFromLibrariesList(librariesList, targetVersion);
                CustomFileDirectoriesObject.WriteTextInFile(libFileName, libListForFile);
            }
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
        void SetCopyLocalFalse(LibraryInfo libFileInfo) {
            XElement elem = libFileInfo.XMLelement;
            XName privat = msbuild + "Private";
            XElement privatElem = elem.Element(privat);
            if (privatElem == null) {
                privatElem = new XElement(privat, "False");
                elem.Add(privatElem);
            }
            else {
                privatElem.SetValue("False");
            }

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
                                      .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress."))
                                      .ToList();
            return lst;
        }
        private void AddLibraryToDocument(XDocument projDocument, List<XElement> xllist, string libraryName) {
            string versionAssemblypattern = @".*(?<version>v\d{2}.\d).*";
            Regex regexVersion = new Regex(versionAssemblypattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var gr = projDocument.Element(msbuild + "Project").Elements(msbuild + "ItemGroup").FirstOrDefault();
            var it = new XElement(gr.Elements().Where(x => x.FirstAttribute.Value.Contains("DevExpress.")).First());
            var at = it.Attribute("Include");
            var val = at.Value;
            var dxLibraryName = val.Split(',')[0];
            Match versionMatch = regexVersion.Match(dxLibraryName);
            var versValue = versionMatch.Groups["version"].Value;
            var newLibraryName = libraryName.Replace("v00.0", versValue);
            var newVal = val.Replace(dxLibraryName, newLibraryName);// "DevExpress.Xpf.Themes.Office2016White.v16.1");
            at.Value = newVal;
            gr.Add(it);
            xllist.Add(it);
        }
        void SetVersion(LibraryInfo libraryInfo, string targetVersion) {
            XElement elem = libraryInfo.XMLelement;

            var atr = elem.Attribute("Include");
            var value = atr.Value;
            //   string versionAssemblypattern = @".*(?<VersionShort>v\d{2}\.\d).*(?<Version>Version=\d{2}\.\d{1}\.\d{1,2}\.0).*";
            string longAssemblyPattern = @".*(?<VersionShort>v\d{2}\.\d).*(?<Version>Version=\d{2}\.\d{1}\.\d{1,2}\.0).*";
            string shortAssemblyPattern = @".*(?<VersionShort>v\d{2}\.\d)";
            Regex regexVersionLong = new Regex(longAssemblyPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Regex regexVersionShort = new Regex(shortAssemblyPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatchLong = regexVersionLong.Match(value);
            Match versionMatchShort = regexVersionShort.Match(value);
            var versValueLong = versionMatchLong.Groups["Version"].Value;
            var versValueShort = versionMatchShort.Groups["VersionShort"].Value;
            string newVersValue = "Version=" + targetVersion + ".0";
            string newShortVersValue = "v" + targetVersion.Substring(0,4);
            value = value.Replace(versValueShort, newShortVersValue);
            libraryInfo.FileName = libraryInfo.FileName.Replace(versValueShort, newShortVersValue);
            if (versionMatchLong.Success) {
                atr.Value = value.Replace(versValueLong, newVersValue);
            }
            else {
                var versValueShortForReplace = newShortVersValue + ", " + newVersValue;
                atr.Value = atr.Value.Replace(newShortVersValue, versValueShortForReplace);
            }
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

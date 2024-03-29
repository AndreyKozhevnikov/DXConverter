﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DXConverter {
    public class AssemblyConverter {
        public ICustomFileDirectories CustomFileDirectoriesObject;
        public IProjectConverterProcessor ProjectConverterProcessorObject;
        public IMessageProcessor MessageProcessor;
        public const string defaultPath = @"\\fs\builds\codecentral\DXDlls\";
        public const string debugPath = @"bin\Debug\";
        public static XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
        public IWorkWithFile MyWorkWithFile;

        internal void ProcessProject(string projectFolder, string version, string installedPath, bool isLocalCache = false) {
            MessageProcessor.SendMessage("Start");

            bool isVersionInstalled;
            Dictionary<string, string> installedVersions;
            string converterPath = null;
            if(!string.IsNullOrEmpty(installedPath)) {
                isVersionInstalled = true;
                converterPath = installedPath;
            } else {
                installedVersions = GetInstalledVersions();
                isVersionInstalled = installedVersions.ContainsKey(version);
                if(isVersionInstalled)
                    converterPath = installedVersions[version];
            }



            List<string> projFiles = GetProjFiles(projectFolder);
            foreach(string projPath in projFiles) {
                RenameBaseImplIn211(projPath, version);
            }
            if(isVersionInstalled) {
                MessageProcessor.SendMessage("Convert to installed version");
                ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                MessageProcessor.SendMessage("Project converter complete");
            } else {
                var dllDirectory = Path.Combine(projectFolder, "DLL");
                converterPath = Path.Combine(defaultPath, version, "ProjectConverter-console.exe");
                if(isLocalCache) {
                    string localCache = @"c:\DllCache\";
                    dllDirectory = Path.Combine(localCache, version);
                    CreateDirectoryDestinationIfNeeded(dllDirectory);
                    var localConverterPath = Path.Combine(dllDirectory, "ProjectConverter-console.exe");
                    if(!CustomFileDirectoriesObject.IsFileExist(localConverterPath)) {
                        CustomFileDirectoriesObject.FileCopy(converterPath, localConverterPath, false);
                        converterPath = localConverterPath;
                    }
                } else {
                    CreateDirectoryDestinationIfNeeded(dllDirectory);
                }
                ProjectConverterProcessorObject.Convert(converterPath, projectFolder);
                MessageProcessor.SendMessage("Project converter complete");

                foreach(string projPath in projFiles) {
                    ProcessCSProjFile(projPath, defaultPath, version, dllDirectory);
                }

            }

            //  MessageProcessor.SendMessage("Finish");
        }
        void RenameBaseImplIn211(string projPath, string targetVersion) {
            var isUnder212 = int.Parse(targetVersion.Split('.')[0].ToString()) < 21 || (int.Parse(targetVersion.Split('.')[0].ToString()) == 21 && int.Parse(targetVersion.Split('.')[1].ToString()) == 1);
            if(isUnder212) {
                var cont = CustomFileDirectoriesObject.GetStringFromFile(projPath);
                cont = cont.Replace("BaseImpl.Xpo", "BaseImpl");
                CustomFileDirectoriesObject.WriteTextInFile(projPath, cont);
            }
        }
        bool IsLibraryExist(string name, List<XElement> libraries) {
            var ind = name.IndexOf("v00.0");
            var searchString = name.Substring(0, ind);
            return libraries.Where(x => x.FirstAttribute.Value.Contains(searchString)).Count() > 0;
        }

        void AddLibraryIfNotExist(string st, List<XElement> libraries, XDocument projDocument) {
            var b = IsLibraryExist(st, libraries);
            if(!b)
                AddLibraryToDocument(projDocument, libraries, st);

        }

        bool GetIsXafWebProject(string projectPaht) {
            return projectPaht.Contains(".Web.") && !projectPaht.Contains(".Module.");
        }
        bool GetIsXafProject(List<XElement> list) {
            var xafLib = list.Where(x => x.Attribute("Include").Value.Contains("ExpressApp"));

            return xafLib.Count() > 0;
        }

        public void ProcessCSProjFile(string projectPath, string sourcePath, string targetVersion, string dllDirectory) {
            CreateOrUpdateProjUserFile(projectPath, dllDirectory);
            XDocument projDocument = CustomFileDirectoriesObject.LoadXDocument(projectPath);
            string libraryDirectory = Path.Combine(sourcePath, targetVersion);
            List<XElement> xlLibraries = GetLibrariesXL(projDocument);
            var isXafWebProj = GetIsXafWebProject(projectPath);
            var isXafProj = GetIsXafProject(xlLibraries);
            var isVersion16 = int.Parse(targetVersion.Split('.')[0].ToString()) >= 16;
            var isUnder212 = int.Parse(targetVersion.Split('.')[0].ToString()) < 21 || (int.Parse(targetVersion.Split('.')[0].ToString()) == 21 && int.Parse(targetVersion.Split('.')[1].ToString()) == 1);
            var requiredLibraries = new List<string>();
            if(isXafWebProj && isVersion16) {
                requiredLibraries.Add("DevExpress.Web.Resources.v00.0");
            }
            if(isXafProj && isUnder212) {
                requiredLibraries.Add("DevExpress.Persistent.BaseImpl.v00.0");
            }
            foreach(string st in requiredLibraries) {
                AddLibraryIfNotExist(st, xlLibraries, projDocument);
            }
            // string directoryDestination = GetDirectoryDesctination(projectPath);

            var libFileName = Path.Combine(dllDirectory, "dxLibraries.txt");
            Dictionary<string, string> existingLibrariesDictionary = GetExistingLibraries(libFileName);
            List<LibraryInfo> librariesList = new List<LibraryInfo>();
            foreach(XElement xl in xlLibraries) {
                string fileName = xl.FirstAttribute.Value.Split(',')[0];
                string assemblyName = fileName + ".dll";

                LibraryInfo libFileInfo = new LibraryInfo();
                libFileInfo.FileName = assemblyName;
                libFileInfo.XMLelement = xl;
                librariesList.Add(libFileInfo);

                //    ChangeHintPath(libFileInfo);
                SetSpecVersion(libFileInfo);
                SetCopyLocalTrue(libFileInfo);
                ProvideReferenceInformation(libFileInfo, targetVersion);
                bool isLibraryAlreadyExist = CheckIfLibraryAlreadyExist(libFileInfo, existingLibrariesDictionary, targetVersion);


                if(!isLibraryAlreadyExist) {
                    string assemblyPath = Path.Combine(libraryDirectory, assemblyName);
                    bool isFileExist = CustomFileDirectoriesObject.IsFileExist(assemblyPath);
                    if(isFileExist) {
                        libFileInfo.FileNameWithPath = assemblyPath;
                        CopyAssemblyCore(dllDirectory, libFileInfo);
                        existingLibrariesDictionary[libFileInfo.FileName] = targetVersion;
                        MessageProcessor.SendMessage(libFileInfo.FileName + " Copied / " + assemblyPath);
                    } else {
                        MessageProcessor.SendMessage(libFileInfo.FileName + " Wrong library", ConsoleColor.Red);
                    }

                } else {
                    MessageProcessor.SendMessage(libFileInfo.FileName + " Skipped", ConsoleColor.Green);
                }
            }
            var libListForFile = GetStringFromLibrariesList(existingLibrariesDictionary);
            CustomFileDirectoriesObject.WriteTextInFile(libFileName, libListForFile);
            CustomFileDirectoriesObject.SaveXDocument(projDocument, projectPath);

        }
        void ProvideReferenceInformation(LibraryInfo libFileInfo, string targetVersion) {
            XElement elem = libFileInfo.XMLelement;
            var attr = elem.FirstAttribute;
            if(!attr.Value.Contains("Version=")) {
                attr.Value = attr.Value + string.Format(", Version={0}.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL", targetVersion);
            }
        }
        void CreateOrUpdateProjUserFile(string projectPath, string dllDirectory) {
            var userFileName = projectPath + ".user";
            var isExist = CustomFileDirectoriesObject.IsFileExist(userFileName);
            if(isExist) {
                XDocument userFileDocument = CustomFileDirectoriesObject.LoadXDocument(userFileName);
                var projectElement = userFileDocument.Element(msbuild + "Project");
                var rootXElement = projectElement.Element(msbuild + "PropertyGroup");
                if(rootXElement == null) {
                    rootXElement = new XElement(msbuild + "PropertyGroup");
                    projectElement.Add(rootXElement);
                }
                var dirPathElement = new XElement(msbuild + "ReferencePath", dllDirectory);
                rootXElement.Add(dirPathElement);
                CustomFileDirectoriesObject.SaveXDocument(userFileDocument, userFileName);
            } else {
                var st = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                         "<Project ToolsVersion = \"15.0\" xmlns = \"http://schemas.microsoft.com/developer/msbuild/2003\">\r\n" +
                         "<PropertyGroup>\r\n" +
                        @"<ReferencePath>" + dllDirectory + "</ReferencePath >\r\n" +
                         "</PropertyGroup>\r\n" +
                         "</Project> \r\n";
                CustomFileDirectoriesObject.FileWriteAllText(userFileName, st);

            }
        }


        Dictionary<string, string> GetInstalledVersions() {//5 td

            var installedVersions = new Dictionary<string, string>();
            List<string> versions = MyWorkWithFile.GetRegistryVersions("SOFTWARE\\DevExpress\\Components\\");
            const string projectUpgradeToolRelativePath = "Tools\\Components\\ProjectConverter-console.exe";
            foreach(string rootPath in versions) {
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
            if(_existingLibrariesDictionary.ContainsKey(_libFileInfo.FileName)) {
                return _existingLibrariesDictionary[_libFileInfo.FileName] == _targetVersion;
            }
            return false;
        }

        public Dictionary<string, string> GetExistingLibraries(string filePath) {
            var st = CustomFileDirectoriesObject.GetStringFromFile(filePath);
            if(st != null) {
                var d = ParseStringToDictionary(st);
                return d;
            }
            return new Dictionary<string, string>();
        }

        public Dictionary<string, string> ParseStringToDictionary(string _string) {
            var list1 = _string.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach(string st in list1) {
                var list2 = st.Split(' ');
                dict[list2[0]] = list2[1];
            }
            return dict;
        }

        public string GetStringFromLibrariesList(Dictionary<string, string> dict) {
            var libListForFile = string.Join(Environment.NewLine, dict.Select(x => x.Key + " " + x.Value));
            //var libListForFile = string.Join(Environment.NewLine, list.Select(x => x.FileName + " " + targetVersion));
            return libListForFile;
        }


        public void ChangeHintPath(LibraryInfo libFileInfo) {
            XElement elem = libFileInfo.XMLelement;
            XName hintPath = msbuild + "HintPath";
            XElement hintPathElem = elem.Element(hintPath);
            string path = debugPath + libFileInfo.FileName;

            if(hintPathElem == null) {
                hintPathElem = new XElement(hintPath, path);
                elem.Add(hintPathElem);
            } else
                hintPathElem.SetValue(path);
        }
        void SetCopyLocalTrue(LibraryInfo libFileInfo) {
            XElement elem = libFileInfo.XMLelement;
            XName privat = msbuild + "Private";
            XElement privatElem = elem.Element(privat);
            if(privatElem == null) {
                privatElem = new XElement(privat, "True");
                elem.Add(privatElem);
            } else {
                privatElem.SetValue("True");
            }

        }
        void SetSpecVersion(LibraryInfo libraryInfo) {
            //XElement elem = libraryInfo.XMLelement;
            //var specName = AssemblyConverter.msbuild + "SpecificVersion";
            //var specVersion = elem.Element(specName);
            //if(specVersion == null) {
            //    specVersion = new XElement(specName, "False");
            //    elem.Add(specVersion);
            //} else {
            //    specVersion.SetValue("False");
            //}
            XElement elem = libraryInfo.XMLelement;
            var specVersion = elem.Element(AssemblyConverter.msbuild + "SpecificVersion");
            if(specVersion != null)
                specVersion.Remove();
        }


        public List<XElement> GetLibrariesXL(XDocument projDocument) {
            List<XElement> lst;
            var frmElement = projDocument.Element(msbuild + "Project");
            if(frmElement != null) {
                 lst = projDocument
                                          .Element(msbuild + "Project")
                                          .Elements(msbuild + "ItemGroup")
                                          .Elements(msbuild + "Reference")
                                          .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress."))
                                          .ToList();
            } else {
                lst = projDocument
                                .Element("Project")
                                .Elements("ItemGroup")
                                .Elements("PackageReference")
                                .Where(elem => elem.FirstAttribute.Value.ToLower().Contains("devexpress."))
                                .ToList();
            }

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
        void SetVersion(LibraryInfo libraryInfo, string targetVersion) { //not used
            XElement elem = libraryInfo.XMLelement;

            var atr = elem.Attribute("Include");
            var value = atr.Value;
            string longAssemblyPattern = @".*(?<VersionShort>v\d{2}\.\d).*(?<Version>Version=\d{2}\.\d{1}\.\d{1,2}\.0).*";
            string shortAssemblyPattern = @".*(?<VersionShort>v\d{2}\.\d)";
            Regex regexVersionLong = new Regex(longAssemblyPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Regex regexVersionShort = new Regex(shortAssemblyPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match versionMatchLong = regexVersionLong.Match(value);
            Match versionMatchShort = regexVersionShort.Match(value);
            var versValueLong = versionMatchLong.Groups["Version"].Value;
            var versValueShort = versionMatchShort.Groups["VersionShort"].Value;
            string newVersValue = "Version=" + targetVersion + ".0";
            string newShortVersValue = "v" + targetVersion.Substring(0, 4);
            value = value.Replace(versValueShort, newShortVersValue);
            libraryInfo.FileName = libraryInfo.FileName.Replace(versValueShort, newShortVersValue);
            if(versionMatchLong.Success) {
                atr.Value = value.Replace(versValueLong, newVersValue);
            } else {
                var versValueShortForReplace = newShortVersValue + ", " + newVersValue;
                atr.Value = atr.Value.Replace(newShortVersValue, versValueShortForReplace);
            }
        }

        public void CreateDirectoryDestinationIfNeeded(string directoryDestination) {
            if(!CustomFileDirectoriesObject.IsDirectoryExist(directoryDestination)) {
                CustomFileDirectoriesObject.CreateDirectory(directoryDestination);
            }
        }

        public List<string> GetProjFiles(string applicationPath) {
            string[] extenshions = new string[] { "*.csproj", "*.vbproj" };
            List<string> projFiles = new List<string>();
            foreach(string extenshion in extenshions) {
                projFiles.AddRange(CustomFileDirectoriesObject.GetFiles(applicationPath, extenshion));
            }
            return projFiles;
        }


        public void CopyAssemblyCore(string directoryDestination, LibraryInfo libFileInfo) {
            string fileDesctination = Path.Combine(directoryDestination, libFileInfo.FileName);
            string fileSource = libFileInfo.FileNameWithPath;
            CustomFileDirectoriesObject.FileCopy(fileSource, fileDesctination, true);
        }
        //public string GetDirectoryDesctination(string projectPath) {
        //    string st = Path.Combine(Directory.GetParent(projectPath).FullName, @"bin\Debug");
        //    return st;
        //}
    }
}

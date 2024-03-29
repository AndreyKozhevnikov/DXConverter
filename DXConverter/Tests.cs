﻿using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DXConverter {
    [TestFixture]
    public class VersionComparer_Tests {
        [Test]
        public void Compare_major1() {
            //arrange
            string st1 = "10.2.15";
            string st2 = "12.2.16";
            VersionComparer comp = new VersionComparer();
            //act
            var res = comp.Compare(st1, st2);
            //assert
            Assert.AreEqual(1, res);
        }
        [Test]
        public void Compare_major2() {
            //arrange
            string st1 = "15.12.15";
            string st2 = "15.5.16";
            VersionComparer comp = new VersionComparer();
            //act
            var res = comp.Compare(st1, st2);
            //assert
            Assert.AreEqual(-1, res);
        }
        [Test]
        public void Compare_minor() {
            //arrange
            string st1 = "15.12.15";
            string st2 = "15.12.16";
            VersionComparer comp = new VersionComparer();
            //act
            var res = comp.Compare(st1, st2);
            //assert
            Assert.AreEqual(1, res);
        }
    }

    [TestFixture]
    public class AssemblyConverter_Test {
        //[Test]
        //public void GetDirectories_Exist() {
        //    //arrange
        //    AssemblyConverter conv = new AssemblyConverter();
        //    var getDirMoq = new Mock<ICustomFileDirectories>();
        //    string[] tmpList = new string[2];
        //    tmpList[0] = "15.1.15";
        //    tmpList[1] = "16.1.4";
        //    getDirMoq.Setup(x => x.GetDirectories(AssemblyConverter.defaultPath)).Returns(tmpList);
        //    conv.CustomFileDirectoriesObject = getDirMoq.Object;
        //    //act
        //    var res = conv.GetVersions();
        //    //assert
        //    Assert.AreEqual(2, res.Count);
        //    Assert.AreEqual("16.1.4", res[0]);
        //    Assert.AreEqual("15.1.15", res[1]);
        //}
        //[Test]
        //public void GetDirectories_notconnection() {
        //    //arrange
        //    AssemblyConverter conv = new AssemblyConverter();
        //    var getDirMoq = new Mock<ICustomFileDirectories>();

        //    getDirMoq.Setup(x => x.GetDirectories(AssemblyConverter.defaultPath)).Throws(new Exception());
        //    conv.CustomFileDirectoriesObject = getDirMoq.Object;
        //    //act
        //    var res = conv.GetVersions();
        //    Console.Write("3");
        //    //assert
        //    Assert.AreEqual(0, res.Count);

        //}


        [Test]
        public void GetProjFiles() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string[] list0 = new string[2];
            list0[0] = "c:\test\test.csproj";
            list0[1] = "c:\test\test1.csproj";
            getDirMoq.Setup(x => x.GetFiles(It.IsAny<String>(), "*.csproj")).Returns(list0);
            string[] list1 = new string[1];
            list1[0] = "c:\test\test.vbproj";
            getDirMoq.Setup(x => x.GetFiles(It.IsAny<String>(), "*.vbproj")).Returns(list1);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var l = conv.GetProjFiles("test");
            //assert
            Assert.AreEqual(3, l.Count);

        }
        [Test]
        public void GetDevExpressElements() {
            //arrange
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            AssemblyConverter conv = new AssemblyConverter();
            //act
            var lst = conv.GetLibrariesXL(xDoc);
            //assert
            Assert.AreEqual(6, lst.Count);
            Assert.AreEqual("<Reference Include=\"Devexpress.Xpf.Grid.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\r\n  <SpecificVersion>False</SpecificVersion>\r\n</Reference>", lst[5].ToString());
        }
        [Test]
        public void GetDevExpressElements_core() {
            //arrange
            string st = Properties.Resources.TestCSprojCore;
            XDocument xDoc = XDocument.Parse(st);
            AssemblyConverter conv = new AssemblyConverter();
            //act
            var lst = conv.GetLibrariesXL(xDoc);
            //assert
            Assert.AreEqual(2, lst.Count);
            Assert.AreEqual("<PackageReference Include=\"DevExpress.ExpressApp.Blazor\" Version=\"22.1.5\" />", lst[0].ToString());
        }

        [Test]
        public void CopyAssemblyCore() {
            //arrange
            string fileName = "testlib.dll";
            string fileNameWithPath = @"c:\source\testlib.dll";
            string destPath = @"c:\tempproject\bin\debug\";
            LibraryInfo li = new LibraryInfo();
            li.FileName = fileName;
            li.FileNameWithPath = fileNameWithPath;
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            //   getDirMoq.Setup(x => x.FileCopy(@"c:\source\t44estlib.dll", @"c:\tempproject\bin\debug\testlib.dll", true));
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            conv.CopyAssemblyCore(destPath, li);
            //assert

            getDirMoq.Verify(x => x.FileCopy(@"c:\source\testlib.dll", @"c:\tempproject\bin\debug\testlib.dll", true));
        }

        [Test]
        public void CreateDirectoryDestinationIfNeeded_Yes() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string dirPath = @"c:\temp\tempproject";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            getDirMoq.Setup(x => x.IsDirectoryExist(dirPath)).Returns(true);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            conv.CreateDirectoryDestinationIfNeeded(dirPath);
            //assert
            getDirMoq.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);

        }
        [Test]
        public void CreateDirectoryDestinationIfNeeded_No() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string dirPath = @"c:\temp\tempproject";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            getDirMoq.Setup(x => x.IsDirectoryExist(dirPath)).Returns(false);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            conv.CreateDirectoryDestinationIfNeeded(dirPath);
            //assert
            getDirMoq.Verify(x => x.CreateDirectory(dirPath), Times.Once);

        }


        [Test]
        public void ChangeHintPath_No() {
            //arrange
            XElement xl = new XElement("TestElement");
            LibraryInfo li = new LibraryInfo();
            li.FileName = "test.dll";
            li.XMLelement = xl;
            AssemblyConverter conv = new AssemblyConverter();
            //act
            conv.ChangeHintPath(li);
            //assert
            XName hintPath = AssemblyConverter.msbuild + "HintPath";

            Assert.AreEqual(1, li.XMLelement.Elements().Count());
            var x = li.XMLelement.Elements().ToList()[0];
            Assert.AreEqual(hintPath, x.Name);
            Assert.AreEqual(AssemblyConverter.debugPath + "test.dll", x.Value);
        }
        [Test]
        public void ChangeHintPath_Yes() {
            //arrange
            XElement xl = new XElement("TestElement");
            XName hintPath = AssemblyConverter.msbuild + "HintPath";
            XElement xlHint = new XElement(hintPath, "testcontent");
            xl.Add(xlHint);
            LibraryInfo li = new LibraryInfo();
            li.FileName = "test.dll";
            li.XMLelement = xl;
            AssemblyConverter conv = new AssemblyConverter();
            //act
            conv.ChangeHintPath(li);
            //assert
            Assert.AreEqual(1, li.XMLelement.Elements().Count());
            var x = li.XMLelement.Elements().ToList()[0];
            Assert.AreEqual(hintPath, x.Name);
            Assert.AreEqual(AssemblyConverter.debugPath + "test.dll", x.Value);
        }

        [Test]
        public void ProcessCSProjFile_LibraryExist() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            XDocument response = null;
            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\Devexpress.Xpf.Grid.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Controls.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Docking.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.GetStringFromFile(It.IsAny<string>())).Returns("Devexpress.Xpf.Grid.v15.2.dll 15.2.5\r\nDevExpress.Xpf.Controls.v15.2.dll 15.2.5");
            getDirMoq.Setup(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath)).Callback<XDocument, string>((x, y) => response = x);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;

            List<string> sendMessageResponse = new List<string>();
            var messMoq = new Mock<IMessageProcessor>();
            messMoq.Setup(x => x.SendMessage(It.IsAny<string>())).Callback<string>(x => sendMessageResponse.Add(x));
            messMoq.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<ConsoleColor>())).Callback<string, ConsoleColor>((x, y) => sendMessageResponse.Add(x));
            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "15.2.5", "");
            var skippedAnswers = sendMessageResponse.Where(x => x.Contains("Skipped") || x.Contains("Wrong library")).ToList();
            var copiedAnswers = sendMessageResponse.Where(x => x.Contains("Copied")).ToList();
            //assert
            getDirMoq.Verify(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath), Times.Once);
            getDirMoq.Verify(x => x.WriteTextInFile(@"dxLibraries.txt", It.IsAny<string>()), Times.Once);
            Assert.AreNotEqual(null, response);
            Assert.AreEqual(6, sendMessageResponse.Count);
            Assert.AreEqual(5, skippedAnswers.Count);
            Assert.AreEqual(1, copiedAnswers.Count);

            XName xName = AssemblyConverter.msbuild + "Reference";
            var doc2 = response.Elements().Elements().SelectMany(x => x.Elements()).Where(x => x.Name == xName).ToList();
            var firstLib = doc2[0];
            var specVersion = firstLib.Element(AssemblyConverter.msbuild + "SpecificVersion");// XName.Get("SpecificVersion", xn.Name.Namespace.NamespaceName));
            Assert.AreEqual(null, specVersion);

            var privatElement = firstLib.Element(AssemblyConverter.msbuild + "Private");
            var privatValue = privatElement.Value;
            Assert.AreEqual("True", privatValue);




        }

        //[Test]
        //public void ProcessCSProjFile_LibraryExist_ShouldBeAllHints() {
        //    //arrange
        //    AssemblyConverter conv = new AssemblyConverter();
        //    string csProjPath = @"c:\test\testproject\testproject.csproj";
        //    var getDirMoq = new Mock<ICustomFileDirectories>();
        //    string st = Properties.Resources.TestCSproj;
        //    XDocument xDoc = XDocument.Parse(st);
        //    XDocument response = null;
        //    getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\Devexpress.Xpf.Grid.v15.2.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Controls.v15.2.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Docking.v15.2.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Data.v15.2.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Printing.v15.2.Core.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Core.v15.2.dll")).Returns(true);
        //    getDirMoq.Setup(x => x.GetStringFromFile(It.IsAny<string>())).Returns("Devexpress.Xpf.Grid.v15.2.dll 15.2.5\r\nDevExpress.Xpf.Controls.v15.2.dll 15.2.5");
        //    getDirMoq.Setup(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath)).Callback<XDocument, string>((x, y) => response = x);

        //    conv.CustomFileDirectoriesObject = getDirMoq.Object;

        //    List<string> sendMessageResponse = new List<string>();
        //    var messMoq = new Mock<IMessageProcessor>();
        //    messMoq.Setup(x => x.SendMessage(It.IsAny<string>())).Callback<string>(x => sendMessageResponse.Add(x));
        //    messMoq.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<ConsoleColor>())).Callback<string, ConsoleColor>((x, y) => sendMessageResponse.Add(x));
        //    conv.MessageProcessor = messMoq.Object;
        //    //act
        //    conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "15.2.5", "");
        //    var skippedAnswers = sendMessageResponse.Where(x => x.Contains("Skipped")).ToList();
        //    var copiedAnswers = sendMessageResponse.Where(x => x.Contains("Copied")).ToList();
        //    //assert
        //    var finalDxLibs = conv.GetLibrariesXL(response);
        //    var libsWithHintPath = finalDxLibs.Where(x => x.Element(AssemblyConverter.msbuild + "HintPath") != null).ToList();

        //    Assert.AreEqual(6, libsWithHintPath.Count);
        //    getDirMoq.Verify(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath), Times.Once);
        //    getDirMoq.Verify(x => x.WriteTextInFile(@"dxLibraries.txt", It.IsAny<string>()), Times.Once);
        //    Assert.AreNotEqual(null, response);
        //    Assert.AreEqual(6, sendMessageResponse.Count);
        //    Assert.AreEqual(2, skippedAnswers.Count);
        //    Assert.AreEqual(4, copiedAnswers.Count);

        //}
        [Test]
        public void ProcessCSProjFile_LibraryExist_CopyParameter() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            XDocument response = null;
            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\Devexpress.Xpf.Grid.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Controls.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Docking.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Data.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Printing.v15.2.Core.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Core.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.GetStringFromFile(It.IsAny<string>())).Returns("Devexpress.Xpf.Grid.v15.2.dll 15.2.5\r\nDevExpress.Xpf.Controls.v15.2.dll 15.2.5");
            getDirMoq.Setup(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath)).Callback<XDocument, string>((x, y) => response = x);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;

            List<string> sendMessageResponse = new List<string>();
            var messMoq = new Mock<IMessageProcessor>();
            messMoq.Setup(x => x.SendMessage(It.IsAny<string>())).Callback<string>(x => sendMessageResponse.Add(x));
            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "15.2.5", "");
            var skippedAnswers = sendMessageResponse.Where(x => x.Contains("Skipped")).ToList();
            var copiedAnswers = sendMessageResponse.Where(x => x.Contains("Copied")).ToList();
            //assert

            getDirMoq.Verify(x => x.FileCopy(AssemblyConverter.defaultPath + @"15.2.5\DevExpress.Xpf.Docking.v15.2.dll", It.IsAny<string>(), true), Times.Once);


        }

        [Test]
        public void ProcessCSProjFile_ProvideFullAttribute() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            XDocument response = null;
            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);

            getDirMoq.Setup(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath)).Callback<XDocument, string>((x, y) => response = x);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            var messMoq = new Mock<IMessageProcessor>();

            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "15.2.2", "");

            //assert
            Assert.True(response.ToString().Contains("DevExpress.Data.v15.2, Version=15.2.2.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL"), response.ToString());
            //getDirMoq.Verify(x => x.FileCopy(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Docking.v15.2.dll", It.IsAny<string>(), true), Times.Once);


        }
        [Test]
        public void GetStringFromLibrariesList() {
            //arrange
            LibraryInfo li0 = new LibraryInfo();
            li0.FileName = "Devexpress.Xpf.Grid.v15.2.dll";
            LibraryInfo li1 = new LibraryInfo();
            li1.FileName = "DevExpress.Xpf.Controls.v15.2.dll";

            var dict = new Dictionary<string, string>();
            dict[li0.FileName] = "15.2.6";
            dict[li1.FileName] = "15.2.6";
            AssemblyConverter conv = new AssemblyConverter();
            string targetString = "Devexpress.Xpf.Grid.v15.2.dll 15.2.6" + Environment.NewLine + "DevExpress.Xpf.Controls.v15.2.dll 15.2.6";
            //act
            var res = conv.GetStringFromLibrariesList(dict);
            //assert
            Assert.AreEqual(targetString, res);
        }

        [Test]
        public void ProcessProject() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var myWorkWithFileMock = new Mock<IWorkWithFile>();
            var listInstalledVersion = new List<string>();
            myWorkWithFileMock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(listInstalledVersion);
            conv.MyWorkWithFile = myWorkWithFileMock.Object;
            string folderPath = @"c:\test\testproject\";
            var procProjMoq = new Mock<IProjectConverterProcessor>();
            string cbProject = null;
            string cbconverter = null;
            procProjMoq.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((x, y) => { cbconverter = x; cbProject = y; });
            conv.ProjectConverterProcessorObject = procProjMoq.Object;

            var getDirMoq = new Mock<ICustomFileDirectories>();
            string csPath = @"c:\test\testproject\testproject.csproj";


            getDirMoq.Setup(x => x.GetFiles(folderPath, "*.csproj")).Returns(new string[] { csPath });
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            getDirMoq.Setup(x => x.LoadXDocument(csPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.GetStringFromFile(csPath)).Returns(Properties.Resources.TestCSproj);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;

            var messMoq = new Mock<IMessageProcessor>();
            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessProject(folderPath, "15.2.3", null);
            //assert
            Assert.AreEqual(folderPath, cbProject);
            Assert.AreEqual(AssemblyConverter.defaultPath + @"15.2.3\ProjectConverter-console.exe", cbconverter);

            // getDirMoq.Verify(x => x.GetFiles(folderPath, "test"), Times.Once);
            getDirMoq.Verify(x => x.GetFiles(folderPath, "*.csproj"), Times.Once);
            getDirMoq.Verify(x => x.GetFiles(folderPath, "*.vbproj"), Times.Once);
            getDirMoq.Verify(x => x.LoadXDocument(csPath), Times.Once);



        }
        [Test]
        public void ProcessProject_ToMajorVersion() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var myWorkWithFileMock = new Mock<IWorkWithFile>();
            var listInstalledVersion = new List<string>();
            listInstalledVersion.Add("C:\\Program Files (x86)\\DevExpress 15.2\\Components\\");
            listInstalledVersion.Add("C:\\Program Files (x86)\\DevExpress 16.1\\Components\\");
            myWorkWithFileMock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(listInstalledVersion);
            myWorkWithFileMock.Setup(x => x.AssemblyLoadFileFullName("C:\\Program Files (x86)\\DevExpress 15.2\\Components\\Tools\\Components\\ProjectConverter-console.exe")).Returns("ProjectConverter-console, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            myWorkWithFileMock.Setup(x => x.AssemblyLoadFileFullName("C:\\Program Files (x86)\\DevExpress 16.1\\Components\\Tools\\Components\\ProjectConverter-console.exe")).Returns("ProjectConverter-console, Version=16.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            conv.MyWorkWithFile = myWorkWithFileMock.Object;
            string folderPath = @"c:\test\testproject\";
            var procProjMoq = new Mock<IProjectConverterProcessor>();
            string cbProject = null;
            string cbconverter = null;
            procProjMoq.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((x, y) => { cbconverter = x; cbProject = y; });
            conv.ProjectConverterProcessorObject = procProjMoq.Object;

            var getDirMoq = new Mock<ICustomFileDirectories>();
            string csPath = @"c:\test\testproject\testproject.csproj";


            getDirMoq.Setup(x => x.GetFiles(folderPath, "*.csproj")).Returns(new string[] { csPath });
            getDirMoq.Setup(x => x.GetStringFromFile(csPath)).Returns(Properties.Resources.TestCSproj);
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            getDirMoq.Setup(x => x.LoadXDocument(csPath)).Returns(xDoc);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;

            var messMoq = new Mock<IMessageProcessor>();
            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessProject(folderPath, "16.1.7", "");
            //assert
            Assert.AreEqual(folderPath, cbProject);
            Assert.AreEqual(@"C:\Program Files (x86)\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", cbconverter);




        }

        [Test]
        public void ProcessProject_ToMajorVersion_passFolder() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var myWorkWithFileMock = new Mock<IWorkWithFile>();
            var listInstalledVersion = new List<string>();
            listInstalledVersion.Add("C:\\Program Files (x86)\\DevExpress 15.2\\Components\\");
            listInstalledVersion.Add("C:\\Program Files (x86)\\DevExpress 16.1\\Components\\");
            myWorkWithFileMock.Setup(x => x.GetRegistryVersions(It.IsAny<string>())).Returns(listInstalledVersion);
            myWorkWithFileMock.Setup(x => x.AssemblyLoadFileFullName("C:\\Program Files (x86)\\DevExpress 15.2\\Components\\Tools\\Components\\ProjectConverter-console.exe")).Returns("ProjectConverter-console, Version=15.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            myWorkWithFileMock.Setup(x => x.AssemblyLoadFileFullName("C:\\Program Files (x86)\\DevExpress 16.1\\Components\\Tools\\Components\\ProjectConverter-console.exe")).Returns("ProjectConverter-console, Version=16.1.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a");
            conv.MyWorkWithFile = myWorkWithFileMock.Object;
            string folderPath = @"c:\test\testproject\";
            var procProjMoq = new Mock<IProjectConverterProcessor>();
            string cbProject = null;
            string cbconverter = null;
            procProjMoq.Setup(x => x.Convert(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((x, y) => { cbconverter = x; cbProject = y; });
            conv.ProjectConverterProcessorObject = procProjMoq.Object;

            var getDirMoq = new Mock<ICustomFileDirectories>();
            string csPath = @"c:\test\testproject\testproject.csproj";


            getDirMoq.Setup(x => x.GetFiles(folderPath, "*.csproj")).Returns(new string[] { csPath });
            getDirMoq.Setup(x => x.GetStringFromFile(csPath)).Returns(Properties.Resources.TestCSproj);
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            getDirMoq.Setup(x => x.LoadXDocument(csPath)).Returns(xDoc);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;

            var messMoq = new Mock<IMessageProcessor>();
            conv.MessageProcessor = messMoq.Object;
            //act
            conv.ProcessProject(folderPath, "16.1.7", @"C:\Program Files (x86)fortest\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe");
            //assert
            Assert.AreEqual(folderPath, cbProject);
            Assert.AreEqual(@"C:\Program Files (x86)fortest\DevExpress 16.1\Components\Tools\Components\ProjectConverter-console.exe", cbconverter);




        }


        [Test]
        public void GetExistingLibraries() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();

            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            conv.GetExistingLibraries("test.txt");
            //assert
            getDirMoq.Verify(x => x.GetStringFromFile("test.txt"), Times.Once);
        }
        [Test]
        public void GetExistingLibraries_Null() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var v = conv.GetExistingLibraries("test.txt");
            //arrange
            Assert.AreEqual(0, v.Count);
        }
        [Test]
        public void GetExistingLibraries_NoNull() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            getDirMoq.Setup(x => x.GetStringFromFile("test.txt")).Returns("test test");
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var v = conv.GetExistingLibraries("test.txt");
            //arrange
            Assert.AreEqual(1, v.Count);
        }
        [Test]
        public void ParseStringToDictionary() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string st = "DevExpress.Data.v15.2.dll 15.2.5";
            st += Environment.NewLine;
            st += "DevExpress.Xpf.Controls.v15.2.dll 15.2.5";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var d = conv.ParseStringToDictionary(st);
            //
            Assert.AreEqual(2, d.Count);
            Assert.AreEqual(true, d.ContainsKey("DevExpress.Data.v15.2.dll"));
            Assert.AreEqual("15.2.5", d["DevExpress.Xpf.Controls.v15.2.dll"]);
        }
        [Test]
        public void CheckIfLibraryAlreadExist() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["DevExpress.Xpf.Controls.v15.2.dll"] = "15.2.3";
            LibraryInfo info = new LibraryInfo() { FileName = "DevExpress.Xpf.Controls.v15.2.dll" };
            LibraryInfo info2 = new LibraryInfo() { FileName = "DevExpress.Data.v15.2.dll" };
            //act
            var b0 = conv.CheckIfLibraryAlreadyExist(info, dict, "15.2.3");
            var b1 = conv.CheckIfLibraryAlreadyExist(info, dict, "15.2.4");
            var b2 = conv.CheckIfLibraryAlreadyExist(info2, dict, "15.2.3");
            //assert
            Assert.AreEqual(true, b0);
            Assert.AreEqual(false, b1);
            Assert.AreEqual(false, b2);
        }

        [Test]
        public void AddRequiredLibraries() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject.Module\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.xafCsproj;
            XDocument xDoc = XDocument.Parse(st);

            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.IsFileExist(AssemblyConverter.defaultPath+ @"16.2.8\DevExpress.Persistent.BaseImpl.v16.2.dll")).Returns(true);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            var messMoq = new Mock<IMessageProcessor>();
            conv.MessageProcessor = messMoq.Object;
            var procProjMoq = new Mock<IProjectConverterProcessor>();
            conv.ProjectConverterProcessorObject = procProjMoq.Object;
            //act
            conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "16.2.8", "");
            //assert
            getDirMoq.Verify(x => x.FileCopy(AssemblyConverter.defaultPath + @"16.2.8\DevExpress.Persistent.BaseImpl.v16.2.dll", It.IsAny<string>(), true), Times.Once);
        }

        [Test]
        public void NotAddRequiredLibrariesToXAF() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.xafCsproj;
            XDocument xDoc = XDocument.Parse(st);

            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Data.v16.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Printing.v16.2.Core.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Xpf.Themes.Office2016White.v16.2.dll")).Returns(true);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            var messMoq = new Mock<IMessageProcessor>();
            conv.MessageProcessor = messMoq.Object;
            var procProjMoq = new Mock<IProjectConverterProcessor>();
            conv.ProjectConverterProcessorObject = procProjMoq.Object;
            //act
            conv.ProcessCSProjFile(csProjPath, AssemblyConverter.defaultPath, "16.2.3", "");
            //assert
            getDirMoq.Verify(x => x.FileCopy(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Data.v16.2.dll", It.IsAny<string>(), true), Times.Never);
            getDirMoq.Verify(x => x.FileCopy(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Printing.v16.2.Core.dll", It.IsAny<string>(), true), Times.Never);
            getDirMoq.Verify(x => x.FileCopy(@"\\CORP\builds\release\DXDlls\16.2.3\DevExpress.Xpf.Themes.Office2016White.v16.2.dll", It.IsAny<string>(), true), Times.Never);
        }


        [Test]
        public void ParametersParser_3() {
            //arrange
            var args = new string[3];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            args[1] = "19.2.4";
            args[2] = "True";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(@"c:\!Tickets\T123123 test sbuject\dx123123", parser.ProjectPath);
            Assert.AreEqual("19.2.4", parser.Version);
            Assert.AreEqual(true, parser.IsWaitForExit);
            Assert.AreEqual(null, parser.InstalledVersionPath);
            Assert.AreEqual(false, parser.IsLocalCacheUsed);
            Assert.AreEqual(true, parser.IsArgumentsCorrect);
        }

        [Test]
        public void ParametersParser_5() {
            //arrange
            var args = new string[5];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            args[1] = "19.2.4";
            args[2] = "True";
            args[3] = @"c:\Program Files (x86)\DevExpress 19.1\Components\Tools\Components\ProjectConverter.exe";
            args[4] = "True";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(@"c:\!Tickets\T123123 test sbuject\dx123123", parser.ProjectPath);
            Assert.AreEqual("19.2.4", parser.Version);
            Assert.AreEqual(true, parser.IsWaitForExit);
            Assert.AreEqual(@"c:\Program Files (x86)\DevExpress 19.1\Components\Tools\Components\ProjectConverter.exe", parser.InstalledVersionPath);
            Assert.AreEqual(true, parser.IsLocalCacheUsed);
            Assert.AreEqual(true, parser.IsArgumentsCorrect);
        }
        [Test]
        public void ParametersParser_4_onlyPath() {
            //arrange
            var args = new string[4];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            args[1] = "19.2.4";
            args[2] = "True";
            args[3] = @"c:\Program Files (x86)\DevExpress 19.1\Components\Tools\Components\ProjectConverter.exe";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(@"c:\!Tickets\T123123 test sbuject\dx123123", parser.ProjectPath);
            Assert.AreEqual("19.2.4", parser.Version);
            Assert.AreEqual(true, parser.IsWaitForExit);
            Assert.AreEqual(@"c:\Program Files (x86)\DevExpress 19.1\Components\Tools\Components\ProjectConverter.exe", parser.InstalledVersionPath);
            Assert.AreEqual(false, parser.IsLocalCacheUsed);
            Assert.AreEqual(true, parser.IsArgumentsCorrect);
        }
        [Test]
        public void ParametersParser_4_onlyCache_true() {
            //arrange
            var args = new string[4];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            args[1] = "19.2.4";
            args[2] = "True";
            args[3] = "True";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(@"c:\!Tickets\T123123 test sbuject\dx123123", parser.ProjectPath);
            Assert.AreEqual("19.2.4", parser.Version);
            Assert.AreEqual(true, parser.IsWaitForExit);
            Assert.AreEqual(null, parser.InstalledVersionPath);
            Assert.AreEqual(true, parser.IsLocalCacheUsed);
            Assert.AreEqual(true, parser.IsArgumentsCorrect);
        }
        [Test]
        public void ParametersParser_4_onlyCache_false() {
            //arrange
            var args = new string[4];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            args[1] = "19.2.4";
            args[2] = "True";
            args[3] = "False";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(@"c:\!Tickets\T123123 test sbuject\dx123123", parser.ProjectPath);
            Assert.AreEqual("19.2.4", parser.Version);
            Assert.AreEqual(true, parser.IsWaitForExit);
            Assert.AreEqual(null, parser.InstalledVersionPath);
            Assert.AreEqual(false, parser.IsLocalCacheUsed);
            Assert.AreEqual(true, parser.IsArgumentsCorrect);
        }

        [Test]
        public void ParametersParser_1() {
            //arrange
            var args = new string[1];
            args[0] = @"c:\!Tickets\T123123 test sbuject\dx123123";
            //act
            var parser = new ParametersParser(args);
            //assert
            Assert.AreEqual(false, parser.IsArgumentsCorrect);
        }


    }
}

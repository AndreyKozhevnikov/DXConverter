using Moq;
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
        [Test]
        public void GetDirectories_Exist() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string[] tmpList = new string[2];
            tmpList[0] = "15.1.15";
            tmpList[1] = "16.1.4";
            getDirMoq.Setup(x => x.GetDirectories(AssemblyConverter.defaultPath)).Returns(tmpList);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var res = conv.GetVersions();
            //assert
            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("16.1.4", res[0]);
            Assert.AreEqual("15.1.15", res[1]);
        }
        [Test]
        public void GetDirectories_notconnection() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();

            getDirMoq.Setup(x => x.GetDirectories(AssemblyConverter.defaultPath)).Throws(new Exception());
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var res = conv.GetVersions();
            Console.Write("3");
            //assert
            Assert.AreEqual(0, res.Count);

        }
        [Test]
        public void GetProjectConverterPath() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            var getDirMoq = new Mock<ICustomFileDirectories>();
            // getDirMoq.Setup(x => x.IsFileExist(It.IsAny<string>())).Returns(true);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var st = conv.GetProjectConverterPath(AssemblyConverter.defaultPath, "16.1.4");

            //assert
            Assert.AreEqual(@"\\CORP\builds\release\DXDlls\16.1.4\ProjectConverter-console.exe", st);
        }

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
            var l = conv.GetProjFiles("test", new string[] { "*.csproj", "*.vbproj" });
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
        public void GetDirectoryDesctination() {
            //arrange
            string projDirectory = @"c:\tempproject\tempproject.csproj";
            AssemblyConverter conv = new AssemblyConverter();
            //act
            string res = conv.GetDirectoryDesctination(projDirectory);
            //assert
            Assert.AreEqual(@"c:\tempproject\bin\Debug", res);
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
        public void GetFullLibrariesInfo() {
            //arrange
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            AssemblyConverter conv = new AssemblyConverter();
            string sourcePath = @"\\CORP\builds\release\DXDlls\15.2.5\";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\Devexpress.Xpf.Grid.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Controls.v15.2.dll")).Returns(true);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var lib = conv.GetFullLibrariesInfo(xDoc, sourcePath);
            //assert
            Assert.AreEqual(2, lib.Count);
            Assert.AreEqual(@"\\CORP\builds\release\DXDlls\15.2.5\Devexpress.Xpf.Grid.v15.2.dll", lib[1].FileNameWithPath);
            Assert.AreEqual(@"Devexpress.Xpf.Grid.v15.2.dll", lib[1].FileName);
            Assert.AreEqual(@"Devexpress.Xpf.Grid.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL", lib[1].XMLelement.Attribute("Include").Value);

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
            Assert.AreEqual(@"bin\Debug\test.dll", x.Value);
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
            Assert.AreEqual(@"bin\Debug\test.dll", x.Value);
        }
        [Test]
        public void CopyAssembliesToProj() {
            //arrange
            AssemblyConverter conv = new AssemblyConverter();
            string csProjPath = @"c:\test\testproject\testproject.csproj";
            var getDirMoq = new Mock<ICustomFileDirectories>();
            string st = Properties.Resources.TestCSproj;
            XDocument xDoc = XDocument.Parse(st);
            XDocument response = null;
            getDirMoq.Setup(x => x.LoadXDocument(csProjPath)).Returns(xDoc);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\Devexpress.Xpf.Grid.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.IsFileExist(@"\\CORP\builds\release\DXDlls\15.2.5\DevExpress.Xpf.Controls.v15.2.dll")).Returns(true);
            getDirMoq.Setup(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath)).Callback<XDocument,string>((x,y) => response=x);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            conv.CopyAssembliesToProj(csProjPath, AssemblyConverter.defaultPath, "15.2.5");
            //assert
            getDirMoq.Verify(x => x.SaveXDocument(It.IsAny<XDocument>(), csProjPath), Times.Once);
            Assert.AreNotEqual(null, response);
        }
    }
}

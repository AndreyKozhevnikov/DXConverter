using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            Assert.AreEqual(0,res.Count);
        
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
            getDirMoq.Setup(x => x.GetFiles(It.IsAny<String>(),"*.csproj")).Returns(list0);
            string[] list1 = new string[1];
            list1[0]= "c:\test\test.vbproj";
            getDirMoq.Setup(x => x.GetFiles(It.IsAny<String>(), "*.vbproj")).Returns(list1);
            conv.CustomFileDirectoriesObject = getDirMoq.Object;
            //act
            var l = conv.GetProjFiles("test",new string[] { "*.csproj", "*.vbproj" });
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
            var lst = conv.GetDevExpressXLElements(xDoc);
            //assert
            Assert.AreEqual(6, lst.Count);
            Assert.AreEqual("<Reference Include=\"devexpress.xpf.grid.v15.2, Version=15.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a, processorArchitecture=MSIL\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\r\n  <SpecificVersion>False</SpecificVersion>\r\n</Reference>", lst[5].ToString());
        }
    }
}

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
   
    }
}

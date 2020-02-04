using NUnit.Framework;

namespace DXConverter {
    [TestFixture]
    public class IFilePathProviderTests {
        [Test]
        public void SimpleFilePathProvider_GetFilePath() {
            //arrange
            var prov = new SimpleFilePathProvider(@"\\CORP\builds\release\DXDlls\", "19.2.6", @"c:\temp\dxT123123\");
            //act
            var res = prov.GetFilePath("test.dll");
            //assert
            Assert.AreEqual(@"\\CORP\builds\release\DXDlls\19.2.6\test.dll", res);
        }
        [Test]
        public void SimpleFilePathProvider_GetDllDirectory() {
            //arrange
            var prov = new SimpleFilePathProvider(@"\\CORP\builds\release\DXDlls\", "19.2.6", @"c:\temp\dxT123123\");
            //act
            var res = prov.GetDllDirectory();
            //assert
            Assert.AreEqual(@"c:\temp\dxT123123\DLL", res);
        }
    }
}

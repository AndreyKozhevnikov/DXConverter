using NUnit.Framework;

namespace DXConverter {
    [TestFixture]
    public class IFilePathProviderTests {
        [Test]
        public void SimpleFilePathProvider() {
            //arrange
            var prov = new SimpleFilePathProvider();
            prov.SourceFolderPath = @"\\CORP\builds\release\DXDlls\";
            prov.Version = "19.2.6";
            //act
            var res = prov.GetFilePath("test.dll");
            //assert
            Assert.AreEqual(@"\\CORP\builds\release\DXDlls\19.2.6\test.dll", res);
        }
    }
}

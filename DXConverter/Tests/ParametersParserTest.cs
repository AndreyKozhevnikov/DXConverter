using NUnit.Framework;

namespace DXConverter {
    [TestFixture]
    public class ParametersParserTest {
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

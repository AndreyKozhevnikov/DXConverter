using NUnit.Framework;
using System.IO;
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
}

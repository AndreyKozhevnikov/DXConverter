using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
    [TestFixture]
    class VersionComparer_Tests {
        [Test]
        public void Compare_1() {
            //arrange
            string st1 = "10.2.15";
            string st2 = "12.2.16";
            VersionComparer comp = new VersionComparer();
            //act
            var res = comp.Compare(st1, st2);
            //assert
            Assert.AreEqual(1, res);
        }
    }
}

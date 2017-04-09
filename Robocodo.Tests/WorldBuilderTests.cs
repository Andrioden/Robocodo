using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocodo.Tests
{
    [TestClass]
    public class WorldBuilderTests
    {
        [TestMethod]
        public void IsWithinWorldTest()
        {
            WorldBuilder worldBuilder = new WorldBuilder(10, 10, 0, null);

            Assert.IsTrue(worldBuilder.IsWithinWorld(5, 5));
            Assert.IsTrue(worldBuilder.IsWithinWorld(0, 0));
            Assert.IsTrue(worldBuilder.IsWithinWorld(0, 9));
            Assert.IsTrue(worldBuilder.IsWithinWorld(9, 0));
            Assert.IsTrue(worldBuilder.IsWithinWorld(9, 9));

            Assert.IsFalse(worldBuilder.IsWithinWorld(-1, 0));
            Assert.IsFalse(worldBuilder.IsWithinWorld(0, -1));
            Assert.IsFalse(worldBuilder.IsWithinWorld(10, 0));
            Assert.IsFalse(worldBuilder.IsWithinWorld(0, 10));
            Assert.IsFalse(worldBuilder.IsWithinWorld(-1, -1));
            Assert.IsFalse(worldBuilder.IsWithinWorld(10, 10));
            Assert.IsFalse(worldBuilder.IsWithinWorld(-100, 100));
        }

    }
}
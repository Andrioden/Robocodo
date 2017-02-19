using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Robocodo.Tests
{
    [TestClass]
    public class MathUtilsTests
    {
        [TestMethod]
        public void DistanceTest()
        {
            Assert.AreEqual(0, MathUtils.Distance(0, 0, 0, 0)); // Same spot
            Assert.AreEqual(3, MathUtils.Distance(0, 0, 0, 3)); // A Line
            Assert.AreEqual(3, MathUtils.Distance(0, 0, 3, 0)); // A Line
        }

        [TestMethod]
        public void LinearConversionTest()
        {
            Assert.AreEqual(0, MathUtils.LinearConversion(0, 200, 0, 100, 0));
            Assert.AreEqual(0, MathUtils.LinearConversion(0, 200, 0, 100, 1));
            Assert.AreEqual(0, MathUtils.LinearConversion(100, 200, 0, 100, 100));
            Assert.AreEqual(20, MathUtils.LinearConversion(0, 10, 0, 100, 2));
            Assert.AreEqual(1, MathUtils.LinearConversion(1, 10, 1, 5, 2));
        }

        [TestMethod]
        public void LinearConversionDoubleTest()
        { 
            Assert.AreEqual(0.5, MathUtils.LinearConversionDouble(0, 200, 0, 100, 1));
        }

        [TestMethod]
        public void LinearConversionInvertedTest()
        {
            Assert.AreEqual(1, MathUtils.LinearConversionInverted(0.0, 1.0, 1));
            Assert.AreEqual(1, MathUtils.LinearConversionInverted(0.0, 2.0, 1));
            Assert.AreEqual(50, MathUtils.LinearConversionInverted(5.0, 10.0, 100));
            Assert.AreEqual(0, MathUtils.LinearConversionInverted(1.0, 1.0, 100));
            Assert.AreEqual(25, MathUtils.LinearConversionInverted(3.0, 4.0, 100));
        }

        [TestMethod]
        public void RoundMin1IfHasValueTest()
        {
            Assert.AreEqual(0, MathUtils.RoundMin1IfHasValue(0.0));
            Assert.AreEqual(1, MathUtils.RoundMin1IfHasValue(0.1));
            Assert.AreEqual(1, MathUtils.RoundMin1IfHasValue(1.1));
            Assert.AreEqual(2, MathUtils.RoundMin1IfHasValue(1.6));
            Assert.AreEqual(2, MathUtils.RoundMin1IfHasValue(2.4));
        }

        [TestMethod]
        public void GenerateNoiseMapTest()
        {
            float[,] noiseMap = MathUtils.GenerateNoiseMap(10, 10);

            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (int y = 0; y < noiseMap.GetLength(1); y++)
                {
                    Console.Write(string.Format("{0} ", noiseMap[x, y]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }
    }
}
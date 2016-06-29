using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void RandomIntTest()
        {
            Assert.AreEqual(0, Utils.RandomInt(0, 0));
            Assert.AreEqual(1, Utils.RandomInt(1, 1));
        }

        [TestMethod()]
        public void RandomDoubleTest()
        {
            Assert.AreEqual(0, Utils.RandomDouble(0, 0));
            Assert.AreEqual(1, Utils.RandomDouble(1, 1));
        }

        [TestMethod()]
        public void PercentageRollTest()
        {
            double percentChance = 0.1 * 2.0;

            double attempts = 1000000;
            double trues = 0;

            for (int i = 0; i < attempts; i++)
            {
                if (Utils.PercentageRoll(percentChance))
                    trues++;
            }

            double measuredPercentChance = trues * 100 / attempts;

            Console.WriteLine("{0} of {1} ({2}%) was 1 with a chance of {3}", trues, attempts, measuredPercentChance, percentChance);

            double measuredVersusInputChanceDif = Math.Abs(measuredPercentChance - percentChance);
            double measuredVersusInputChanceDifFactor = measuredVersusInputChanceDif / percentChance;

            Assert.IsTrue(measuredVersusInputChanceDifFactor < 0.2, "Measured chance is not very equal to input chance, diference factor is " + measuredVersusInputChanceDifFactor);
        }

    }
}
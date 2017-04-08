using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocodo.Tests
{
    [TestClass]
    public class ValueTrackerTests
    {
        
        [TestMethod]
        public void ValueTrackerTest()
        {
            ValueTracker valueTracker = new ValueTracker(0.9, 0);

            valueTracker.AddDataPoint(1, 10);
            Assert.AreEqual(9, valueTracker.ChangePerTick);

            //valueTracker.AddDataPoint(2, 20);
            //Assert.AreEqual(9, valueTracker.ChangePerTick);
        }

    }
}
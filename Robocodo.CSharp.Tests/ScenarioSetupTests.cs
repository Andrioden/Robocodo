using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robocodo.CSharp;

namespace Robocodo.CSharp.Tests
{
    [TestClass]
    public class ScenarioSetupTests
    {

        [TestMethod]
        public void ValidateScenarioEnum()
        {
            int previousEnumValue = -1; // Kinda not true, but since it starts at 0, the previous kinda is -1
            foreach (Scenario scenario in Enum.GetValues(typeof(Scenario)))
            {
                int enumValue = (int)scenario;
                Assert.AreEqual(1, enumValue - previousEnumValue);
                previousEnumValue = enumValue;
            }
        }

    }
}

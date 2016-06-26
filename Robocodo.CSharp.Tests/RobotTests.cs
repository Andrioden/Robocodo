using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robocodo.CSharp;

namespace Robocodo.CSharp.Tests
{
    [TestClass]
    public class RobotTests
    {
        [TestMethod]
        public void RobotInstructionTest()
        {
            Assert.IsFalse(Instructions.IsValidInstruction("LOL"));
            Assert.IsFalse(Instructions.IsValidInstruction("move left"));

            Assert.IsTrue(Instructions.IsValidInstruction("MOVE UP"));
            Assert.IsTrue(Instructions.IsValidInstruction("MOVE DOWN"));
            Assert.IsTrue(Instructions.IsValidInstruction("MOVE LEFT"));
            Assert.IsTrue(Instructions.IsValidInstruction("MOVE RIGHT"));
        }
    }
}

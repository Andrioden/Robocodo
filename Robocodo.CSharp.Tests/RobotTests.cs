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
            Assert.IsFalse(Instruction.IsValidInstruction("LOL"));
            Assert.IsFalse(Instruction.IsValidInstruction("move left"));

            Assert.IsTrue(Instruction.IsValidInstruction("MOVE UP"));
            Assert.IsTrue(Instruction.IsValidInstruction("MOVE DOWN"));
            Assert.IsTrue(Instruction.IsValidInstruction("MOVE LEFT"));
            Assert.IsTrue(Instruction.IsValidInstruction("MOVE RIGHT"));
        }
    }
}

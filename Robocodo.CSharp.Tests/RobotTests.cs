using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robocodo.CSharp;

namespace Robocodo.CSharp.Tests
{
    [TestClass]
    public class InstructionsTests
    {

        [TestMethod]
        public void IsValidLoopStartTest()
        {
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP STRT"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START ("));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START )"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () )"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () ("));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () ()"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a23)"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (23) (23)"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a/23)"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a/b)"));
            Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (23/24/25)"));

            Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START"));
            Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (1)"));
            Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (44)"));
            Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (44/45)"));
        }

        [TestMethod]
        public void IsLoopStartCompletedTest()
        {
            Assert.IsFalse(Instructions.IsLoopStartCompleted("LOOP START (44/45)"));
            Assert.IsTrue(Instructions.IsLoopStartCompleted("LOOP START (45/45)"));
        }

        [TestMethod]
        public void GetStringInsideParenthesesTest()
        {
            Assert.AreEqual("a123", Instructions.GetParenthesesContent("LOOP START (a123)"));
        }

        //[TestMethod]
        //public void RobotInstructionTest()
        //{
        //    Assert.IsFalse(Instructions.IsValidInstruction("LOL"));
        //    Assert.IsFalse(Instructions.IsValidInstruction("move left"));

        //    Assert.IsTrue(Instructions.IsValidInstruction("MOVE UP"));
        //    Assert.IsTrue(Instructions.IsValidInstruction("MOVE DOWN"));
        //    Assert.IsTrue(Instructions.IsValidInstruction("MOVE LEFT"));
        //    Assert.IsTrue(Instructions.IsValidInstruction("MOVE RIGHT"));
        //}
    }
}

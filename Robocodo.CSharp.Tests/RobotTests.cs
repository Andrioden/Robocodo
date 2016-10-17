using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robocodo.CSharp;

namespace Robocodo.CSharp.Tests
{
    [TestClass]
    public class InstructionsTests
    {

        //[TestMethod]
        //public void IsValidLoopStartTest()
        //{
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP STRT"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START ("));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START )"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () )"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () ("));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START () ()"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a23)"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (23) (23)"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a/23)"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (a/b)"));
        //    Assert.IsFalse(Instructions.IsValidLoopStart("LOOP START (23/24/25)"));

        //    Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START"));
        //    Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (1)"));
        //    Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (44)"));
        //    Assert.IsTrue(Instructions.IsValidLoopStart("LOOP START (44/45)"));
        //}

        //[TestMethod]
        //public void IsLoopStartCompletedTest()
        //{
        //    Assert.IsFalse(Instructions.IsLoopStartCompleted("LOOP START (44/45)"));
        //    Assert.IsTrue(Instructions.IsLoopStartCompleted("LOOP START (45/45)"));
        //}

        //[TestMethod]
        //public void IsValidDetectTest()
        //{
        //    Assert.IsTrue(Instructions.IsValidDetectThen("DETECT ENEMY THEN MOVE HOME"));
        //    Assert.IsTrue(Instructions.IsValidDetectThen("DETECT COPPER THEN MOVE HOME"));
        //    Assert.IsTrue(Instructions.IsValidDetectThen("DETECT IRON THEN MOVE HOME"));
        //    Assert.IsTrue(Instructions.IsValidDetectThen("DETECT COPPER THEN HARVEST"));

        //    Assert.IsFalse(Instructions.IsValidDetectThen("LOL DETECT ENEMY THEN MOVE HOME"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("DETECT"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("DETECT ENEMY"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("DETECT ENEMY THEN"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("DETECT ENEMY THEN "));
        //}

        //[TestMethod]
        //public void IsValidIdleUntilTest()
        //{
        //    Assert.IsTrue(Instructions.IsValidIdleUntil("IDLE UNTIL FULL THEN MOVE HOME"));
        //    Assert.IsTrue(Instructions.IsValidIdleUntil("UDLE UNTIL FULL THEN MOVE LEFT"));

        //    Assert.IsFalse(Instructions.IsValidDetectThen("IDLE FULL THEN MOVE HOME"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("IDLE UNTIL FULLJ THEN MOVE HOME"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("IDLE UNTIL STUPID THEN MOVE HOME"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("IDLES UNTIL FULL THEN MOVE HOME"));
        //    Assert.IsFalse(Instructions.IsValidDetectThen("IDLEUNTIL FULL THEN MOVE HOME"));
        //}

        [TestMethod]
        public void GetStringAfterSpaceTest()
        {
            Assert.AreEqual("MOVE HOME", InstructionsHelper.GetStringAfterSpace("DETECT COPPER THEN MOVE HOME", 3));
            Assert.AreEqual("Two Three Four", InstructionsHelper.GetStringAfterSpace("One Two Three Four", 1));
            Assert.AreEqual("Three Four", InstructionsHelper.GetStringAfterSpace("One Two Three Four", 2));
            Assert.AreEqual("Four", InstructionsHelper.GetStringAfterSpace("One Two Three Four", 3));
        }
    }
}

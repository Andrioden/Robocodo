using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocodo.Tests
{
    [TestClass]
    public class InstructionTests
    {
        [TestMethod]
        public void FindLoopStartPairedEndIndex_FindLoopEndPairedStartIndex_Test()
        {
            List<Instruction> instructions = new List<Instruction>()
            {
                new Instruction_LoopStart(),
                    new Instruction_Harvest(),
                    new Instruction_LoopStart(),
                        new Instruction_Harvest(),
                    new Instruction_LoopEnd(),
                new Instruction_LoopEnd(),
                new Instruction_LoopStart(),
            };

            Assert.AreEqual(5, Instruction_LoopStart.FindLoopStartPairedEndIndex(instructions, 0));
            Assert.AreEqual(4, Instruction_LoopStart.FindLoopStartPairedEndIndex(instructions, 2));
            Assert.AreEqual(-1, Instruction_LoopStart.FindLoopStartPairedEndIndex(instructions, 6));

            Assert.AreEqual(0, Instruction_LoopEnd.FindLoopEndPairedStartIndex(instructions, 5));
            Assert.AreEqual(2, Instruction_LoopEnd.FindLoopEndPairedStartIndex(instructions, 4));
        }
    }
}
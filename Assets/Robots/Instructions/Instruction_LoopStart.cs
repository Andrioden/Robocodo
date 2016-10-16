using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class Instruction_LoopStart : Instruction
{

    public static readonly string SerializedType = "LOOP START";

    private RobotController robot;
    private int iterations;
    private int currentIteration;

    public Instruction_LoopStart(int iterations = 0)
    {
        this.iterations = iterations;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;

        IterateCounterIfNeeded();
        robot.ResetAllInnerLoopStarts(robot.NextInstructionIndex + 1);

        return true;
    }

    public override bool CanBePreviewed()
    {
        return true;
    }

    public override string Serialize()
    {
        if (iterations == 0)
            return SerializedType;
        else
            return string.Format("{0} {1}/{2}", SerializedType, currentIteration, iterations);
    }

    public static bool IsValid(string instruction)
    {
        if (instruction == "LOOP START")
            return true;
        if (Regex.Match(instruction, @"^LOOP START \d+$").Success) // Understand regex better: https://regex101.com/r/lY9pP4/2
            return true;
        if (Regex.Match(instruction, @"^LOOP START \d+\/\d+$").Success) // Understand regex better: https://regex101.com/r/bR3kG5/2
            return true;
        return false;
    }

    private void IterateCounterIfNeeded()
    {
        if (iterations == 0)
            return;
        else
            currentIteration++;

        //TODO: What do I have to do here?
        //instructions[nextInstructionIndex] = Instructions.LoopStartNumberedSet(currentLoopCount, totalLoopCount);
        //instructions.Dirty(nextInstructionIndex);





        //string loopNumber = instruction.Replace(Instructions.LoopStart, "").Trim();
        //string[] loopNumberSplit = loopNumber.Split('/');

        //int currentLoopCount = -1;
        //int totalLoopCount = -1;

        //if (loopNumberSplit.Length == 1)
        //{
        //    // First time running Loop
        //    currentLoopCount = 1;
        //    totalLoopCount = Convert.ToInt32(loopNumberSplit[0]);
        //}
        //else if (loopNumberSplit.Length == 2)
        //{
        //    // Loop has been run before, example 'LOOP START (1/2)' means that it has been run 1 of 2 times
        //    currentLoopCount = Convert.ToInt32(loopNumberSplit[0]) + 1;
        //    totalLoopCount = Convert.ToInt32(loopNumberSplit[1]);
        //}
        //else
        //    throw new Exception("Illegal amount of forward slashes in instruction: " + instruction);

        //instructions[nextInstructionIndex] = Instructions.LoopStartNumberedSet(currentLoopCount, totalLoopCount);
        //instructions.Dirty(nextInstructionIndex);
    }
}
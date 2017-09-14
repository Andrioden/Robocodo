using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class Instruction_LoopStart : Instruction
{

    public override int Setting_EnergyCost() { return 0; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return false; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return null; }
    public override bool CanBeExecutedForPreviewRobot() { return true; }

    public static readonly string Format = "LOOP START";

    private RobotController robot;
    private int iterations;
    private int currentIteration;

    public Instruction_LoopStart(int iterations = 0)
    {
        this.iterations = iterations;
    }

    public Instruction_LoopStart(int currentIteration, int iterations)
    {
        this.currentIteration = currentIteration;
        this.iterations = iterations;
    }

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;

        IterateCounterIfNeeded();
        robot.ResetAllInnerLoopStarts(robot.nextInstructionIndex + 1);

        return true;
    }

    public override string Serialize()
    {
        if (iterations == 0)
            return Format;
        else
            return string.Format("{0} {1}/{2}", Format, currentIteration, iterations);
    }

    public static Instruction Deserialize(string instruction)
    {
        if (instruction == Format)
            return new Instruction_LoopStart();

        string loopNumber = instruction.Replace(Format, "").Trim();
        string[] loopNumberSplit = loopNumber.Split('/');

        if (loopNumberSplit.Length == 1)
            return new Instruction_LoopStart(0, Convert.ToInt32(loopNumberSplit[0]));
        else if (loopNumberSplit.Length == 2)
            return new Instruction_LoopStart(Convert.ToInt32(loopNumberSplit[0]), Convert.ToInt32(loopNumberSplit[1]));
        else
            throw new Exception("Unknown loop start instruction format, should not reach this code as it should be validated ahead: " + instruction);
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

    public bool IsIterationsCompleted()
    {
        return iterations != 0 && currentIteration == iterations;
    }

    public void ResetCurrentIterations()
    {
        currentIteration = 0;
        robot.NotifyInstructionsChanged();
    }

    private void IterateCounterIfNeeded()
    {
        if (iterations == 0)
            return;
        else
        {
            currentIteration++;
            robot.NotifyInstructionsChanged();
        }
    }
}
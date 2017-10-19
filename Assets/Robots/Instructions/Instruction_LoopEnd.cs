using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_LoopEnd : Instruction
{

    public override int Setting_EnergyCost() { return 0; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return false; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return null; }
    public override bool CanBeExecutedForPreviewRobot() { return true; }

    public static readonly string Format = "LOOP END";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        SetNextInstructionToPairedLoopStartIfNotCompleted();
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_LoopEnd();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

    private void SetNextInstructionToPairedLoopStartIfNotCompleted()
    {
        int loopStartIndex = FindLoopEndPairedStartIndex(robot.Instructions, robot.nextInstructionIndex);

        if (loopStartIndex == -1)
            robot.SetFeedback("COULD NOT FIND MATCHING LOOP START", false, false);
        else
        {
            Instruction_LoopStart loopStartInstruction = (Instruction_LoopStart)robot.Instructions[loopStartIndex];
            if (loopStartInstruction.IsIterationsCompleted())
                return;
            else
            {
                loopStartInstruction.IterateCounterIfNeeded();
                robot.NotifyInstructionsChanged();
                robot.nextInstructionIndex = loopStartIndex - 1;
                return;
            }
        }
    }

    /// <summary>
    /// Returns the LOOP START index for the given LOOP END index
    /// </summary>
    public static int FindLoopEndPairedStartIndex(List<Instruction> instructions, int loopEndIndex)
    {
        int skippingLoopStarts = 0;
        for (int i = loopEndIndex - 1; i >= 0; i--)
        {
            if (instructions[i].GetType() == typeof(Instruction_LoopEnd))
                skippingLoopStarts++;
            else if (instructions[i].GetType() == typeof(Instruction_LoopStart))
            {
                if (skippingLoopStarts == 0)
                    return i;
                else
                    skippingLoopStarts--;
            }
        }

        return -1;
    }

}
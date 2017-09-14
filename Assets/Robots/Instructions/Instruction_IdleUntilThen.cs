using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class Instruction_IdleUntilThen : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return true; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return null; }
    public override bool CanBeExecutedForPreviewRobot() { return false; }

    public static readonly string Format = "IDLE UNTIL [WHAT] THEN [INSTRUCTION]";

    private UntilWhat untilWhat;
    private Instruction thenInstruction;

    public Instruction_IdleUntilThen(UntilWhat untilWhat, Instruction thenInstruction)
    {
        this.untilWhat = untilWhat;
        this.thenInstruction = thenInstruction;
    }

    public override bool Execute(RobotController robot)
    {
        if (untilWhat == UntilWhat.Full && !robot.IsInventoryFull())
            return false;

        return thenInstruction.Execute(robot);
    }

    public override string Serialize()
    {
        string serialized = Format.Replace("[WHAT]", untilWhat.ToString().ToUpper());
        if (thenInstruction != null)
            serialized = serialized.Replace("[INSTRUCTION]", thenInstruction.Serialize());
        return serialized;
    }

    public static Instruction Deserialize(string instruction)
    {
        string thenInstructionString = InstructionsHelper.GetStringAfterSpace(instruction, 4);
        Instruction thenInstruction = InstructionsHelper.Deserialize(thenInstructionString);

        string untilWhatString = instruction.Split(' ')[2];
        UntilWhat untilWhat = Utils.ParseEnum<UntilWhat>(untilWhatString);

        return new Instruction_IdleUntilThen(untilWhat, thenInstruction);
    }

    public static bool IsValid(string instruction)
    {
        if (Regex.Match(instruction, @"^IDLE UNTIL \b(FULL)\b THEN .+$").Success) // Understand regex better: https://regex101.com/r/rU4dK3/1
        {
            string thenInstructionString = InstructionsHelper.GetStringAfterSpace(instruction, 4);
            Instruction thenInstruction = InstructionsHelper.Deserialize(thenInstructionString);

            return InstructionsHelper.IsValidConditionaledInstruction(thenInstruction);
        }
        return false;
    }

}

public enum UntilWhat
{
    Full
}
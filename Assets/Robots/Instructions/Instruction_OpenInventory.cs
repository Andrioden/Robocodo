using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_OpenInventory : Instruction
{

    public override int Setting_EnergyCost() { return 0; }
    public override bool Setting_Still() { return true; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return true; }
    public override PreviewImage Setting_PreviewImage() { return null; }
    public override bool CanBeExecutedForPreviewRobot() { return true; }

    public static readonly string Format = "OPEN INVENTORY";

    public override bool Execute(RobotController robot)
    {
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_OpenInventory();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

}
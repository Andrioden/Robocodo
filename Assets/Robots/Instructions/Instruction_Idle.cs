using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Idle : Instruction
{

    public override int Setting_EnergyCost() { return 0; }

    public static readonly string Format = "IDLE";

    public override bool Execute(RobotController robot)
    {
        return true;
    }

    public override bool CanBePreviewed()
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
            return new Instruction_Idle();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

}
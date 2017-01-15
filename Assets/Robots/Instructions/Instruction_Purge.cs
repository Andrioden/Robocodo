using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Purge : Instruction
{

    public static readonly string Format = "PURGE";

    public override bool Execute(RobotController robot)
    {
        return InfectionManager.instance.DecreaseTileInfection(robot, Settings.Robot_Purge_InfectionReducedPerTick);
    }

    public override bool CanBePreviewed()
    {
        return false;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_Purge();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

}
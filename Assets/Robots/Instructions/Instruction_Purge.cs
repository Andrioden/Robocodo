using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Purge : Instruction
{

    public override int Setting_EnergyCost() { return 1; }

    public static readonly string Format = "PURGE";

    public override bool Execute(RobotController robot)
    {
        double oldInfection = InfectionManager.instance.GetTileInfection(robot.X, robot.Z);
        double newInfection = InfectionManager.instance.DecreaseTileInfection(robot.X, robot.Z, robot.Owner, Settings.Robot_Purge_InfectionReducedPerTick);

        int infectionDecreased = (int)Math.Floor(oldInfection - newInfection);
        robot.Owner.TechTree.AddProgressToActiveResearch(infectionDecreased);

        return newInfection <= 0;
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
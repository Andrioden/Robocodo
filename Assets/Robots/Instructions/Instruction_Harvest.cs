using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Harvest : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool CanBePreviewed() { return false; }

    public static readonly string Format = "HARVEST";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        Harvest();
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_Harvest();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

    private void Harvest()
    {
        for (int i = 0; i < robot.Settings_HarvestYield(); i++)
        {
            if (robot.Settings_InventoryCapacity() == 0) {
                robot.SetFeedback("NO INVENTORY CAPACITY", true, false);
                return;
            }
            else if (robot.IsInventoryFull()) {
                robot.SetFeedback("INVENTORY FULL", true, false);
                return;
            }

            string resourceType = WorldController.instance.HarvestFromNode(robot.X, robot.Z);

            if (resourceType != null)
                robot.TransferToInventory(InventoryItem.DeserializeType(resourceType));
            else
                robot.SetFeedback("NOTHING TO HARVEST", true, false);
        }

        if (robot.Settings_HarvestYield() == 0)
            robot.SetFeedback("NO HARVEST YIELD", true, false);
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Instruction_Harvest : Instruction
{

    public static readonly string Format = "HARVEST";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        Harvest();
        return true;
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
            if (robot.Settings_InventoryCapacity() == 0)
                robot.SetFeedbackIfNotPreview("NO INVENTORY CAPACITY", true, true);
            else if (robot.IsInventoryFull())
                robot.SetFeedbackIfNotPreview("INVENTORY FULL", true, true);

            string resourceType = WorldController.instance.HarvestFromNode(robot.x, robot.z);

            if (resourceType != null)
                robot.TransferToInventory(InventoryItem.DeserializeType(resourceType));
            else
                robot.SetFeedbackIfNotPreview("NOTHING TO HARVEST", true, true);
        }

        if (robot.Settings_HarvestYield() == 0)
            robot.SetFeedbackIfNotPreview("NO HARVEST YIELD", true, true);
    }

}
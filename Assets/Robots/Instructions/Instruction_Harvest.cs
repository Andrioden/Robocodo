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

    private void Harvest()
    {
        for (int i = 0; i < robot.Settings_HarvestYield(); i++)
        {
            if (robot.Settings_InventoryCapacity() == 0)
                robot.SetFeedbackIfNotPreview("NO INVENTORY CAPACITY");
            else if (robot.IsInventoryFull())
                robot.SetFeedbackIfNotPreview("INVENTORY FULL");
            else if (WorldController.instance.HarvestFromNode(CopperItem.SerializedType, robot.x, robot.z))
                robot.TransferToInventory(new CopperItem());
            else if (WorldController.instance.HarvestFromNode(IronItem.SerializedType, robot.x, robot.z))
                robot.TransferToInventory(new IronItem());
            else
                robot.SetFeedbackIfNotPreview("NOTHING TO HARVEST");
        }

        if (robot.Settings_HarvestYield() == 0)
            robot.SetFeedbackIfNotPreview("NO HARVEST YIELD");
    }

}
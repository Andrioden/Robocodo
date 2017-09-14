using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_DropInventory : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override PreviewImage Setting_PreviewImage() { return new PreviewImage { Name = "Wheelbarrow", VerticalAlign = VerticalAlign.Top }; }
    public override bool CanBeExecutedForPreviewRobot() { return false; }

    public static readonly string Format = "DROP INVENTORY";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        DropInventory();
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_DropInventory();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

    private void DropInventory()
    {
        IHasInventory droppableTarget = robot.FindAllOnCurrentPosition<IHasInventory>().FirstOrDefault(r => r.HasOpenInventory());

        if (droppableTarget != null)
        {
            List<InventoryItem> itemsNotAdded = droppableTarget.AddToInventory(robot.Inventory, true);
            robot.SetInventory(itemsNotAdded);
            if (itemsNotAdded.Count > 0)
                robot.SetFeedback("NOT ALL ITEMS DROPPED; TARGET FULL", true, false);
        }
        else
            Debug.Log("SERVER: No droppable, should drop items on ground. Not fully implemented.");
    }

}
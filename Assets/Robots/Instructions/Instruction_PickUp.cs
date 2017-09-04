using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_PickUp : Instruction
{

    public override int Setting_EnergyCost() { return 1; }
    public override bool Setting_Still() { return false; }
    public override bool Setting_ConsumesTick() { return true; }
    public override bool Setting_AllowStacking() { return false; }
    public override bool CanBePreviewed() { return false; }

    public static readonly string Format = "PICK UP";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        PickUp();
        return true;
    }

    public override string Serialize()
    {
        return Format;
    }

    public static Instruction Deserialize(string instruction)
    {
        if (IsValid(instruction))
            return new Instruction_PickUp();
        else
            throw new Exception(string.Format("Tried to deserialize an {0} instruction that wasnt valid.", Format));
    }

    public static bool IsValid(string instruction)
    {
        return instruction == Format;
    }

    private void PickUp()
    {
        List<IHasInventory> droppableTargets = robot.FindAllOnCurrentPosition<IHasInventory>();

        foreach(IHasInventory droppableTarget in droppableTargets)
        {
            List<InventoryItem> pickedUpItems = droppableTarget.PickUp(robot.Settings_InventoryCapacity() - robot.Inventory.Count());
            List<InventoryItem> itemsNotAdded = robot.AddToInventory(pickedUpItems, false);

            if (itemsNotAdded.Count > 0)
                throw new Exception("Was not able to add all picked up items to the inventory. Should not logically happen.");
        }

        if (droppableTargets.Count == 0)
            robot.SetFeedback("NO PICK UP TARGET", true, true);
    }

}
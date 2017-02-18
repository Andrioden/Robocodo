using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_DropInventory : Instruction
{

    public override int Setting_EnergyCost() { return 1; }

    public static readonly string Format = "DROP INVENTORY";

    private RobotController robot;

    public override bool Execute(RobotController robot)
    {
        this.robot = robot;
        DropInventory();
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
        IHasInventory droppableTarget = FindDroppableTargetPrioritized();

        if (droppableTarget != null)
        {
            List<InventoryItem> itemsNotAdded = droppableTarget.AddToInventory(robot.Inventory);
            robot.SetInventory(itemsNotAdded);
            if (itemsNotAdded.Count > 0)
                robot.SetFeedback("NOT ALL ITEMS DROPPED, TARGET FULL", true, true);
        }
        else
            Debug.Log("SERVER: No droppable, should drop items on ground. Not fully implemented.");
    }

    private IHasInventory FindDroppableTargetPrioritized()
    {
        IHasInventory storageRobot = robot.FindFirstOnCurrentPosition<StorageRobotController>();
        if (storageRobot != null)
            return storageRobot;

        IHasInventory transporterRobot = robot.FindFirstOnCurrentPosition<TransporterRobotController>();
        if (transporterRobot != null)
            return transporterRobot;

        return robot.FindFirstOnCurrentPosition<IHasInventory>();
    }

}
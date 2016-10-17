using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Instruction_DropInventory : Instruction
{

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

    private void DropInventory()
    {
        IHasInventory droppableTarget = FindDroppableTarget((int)robot.x, (int)robot.z);
        if (droppableTarget != null)
        {
            List<InventoryItem> itemsNotAdded = droppableTarget.TransferToInventory(robot.Inventory);
            robot.SetInventory(itemsNotAdded);
            if (itemsNotAdded.Count > 0)
                robot.SetFeedbackIfNotPreview("NOT ALL ITEMS DROPPED, TARGET FULL");
        }
        else
            Debug.Log("SERVER: No droppable, should drop items on ground. Not fully implemented.");
    }

    private IHasInventory FindDroppableTarget(int x, int z)
    {
        foreach (GameObject potentialGO in robot.FindNearbyCollidingGameObjects<IHasInventory>())
        {
            IHasInventory droppable = potentialGO.transform.root.GetComponent<IHasInventory>();
            if (potentialGO.transform.position.x == x && potentialGO.transform.position.z == z)
                return droppable;
        }

        Debug.Log("Did not find attackable");
        return null;
    }
}
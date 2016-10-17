using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransporterRobotController : RobotController
{

    public static string Settings_name = "TRANSPORTER";
    public static int Settings_copperCost = 5;
    public static int Settings_ironCost = 20;
    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    public override string Settings_Name() { return Settings_name; }
    public override int Settings_CopperCost() { return Settings_copperCost; }
    public override int Settings_IronCost() { return Settings_ironCost; }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 2; }
    public override int Settings_MaxEnergy() { return 200; }
    public override int Settings_InventoryCapacity() { return 10; }
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        new Instruction_DropInventory(),
        new Instruction_IdleUntilThen(UntilWhat.Full, null),
    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {
            new Instruction_Idle(),
        };
    }

    protected override void Animate()
    {

    }


    public override GameObject SpawnPreviewGameObjectClone()
    {
        return (GameObject)Instantiate(WorldController.instance.harvesterRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageRobotController : RobotController
{
    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "STORAGE";
    public static Cost Settings_cost() { return new Cost() { Copper = 10, Iron = 30 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.gray; }
    public override Cost Settings_Cost() { return Settings_cost(); }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 200; }
    public override int Settings_InventoryCapacity() { return 50; }
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        //Nothing
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
        if (!meshGO.activeSelf)
            return;
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return Instantiate(WorldController.instance.storageRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

}

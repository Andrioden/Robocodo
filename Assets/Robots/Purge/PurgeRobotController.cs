using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PurgeRobotController : RobotController
{

    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "PURGER";
    public static Cost Settings_cost() { return new Cost() { Copper = 1, Iron = 1 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.green; }
    public override Cost Settings_Cost() { return Settings_cost(); }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 200; }
    public override int Settings_InventoryCapacity() { return 0; }
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        new Instruction_Purge()
    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {
            new Instruction_LoopStart(10),
            new Instruction_Move(MoveDirection.Random),
            new Instruction_Purge(),
            new Instruction_LoopEnd(),
            new Instruction_Move(MoveDirection.Home)
        };
    }

    protected override void Animate()
    {
        if (!meshGO.activeSelf)
            return;
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return Instantiate(WorldController.instance.purgeRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryRobotController : RobotController, IEnergySource
{

    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "BATTERY";
    public static Cost Settings_cost() { return new Cost() { Copper = 100, Iron = 50 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.magenta; }
    public override Cost Settings_Cost() { return Settings_cost(); }
    public override int Settings_Memory() { return 15; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 1000; }
    public override int Settings_InventoryCapacity() { return 0; }
    public override int Settings_ModuleCapacity() { return 3; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {

    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {

        };
    }

    protected override void Animate()
    {
        if (!meshGO.activeSelf)
            return;
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return Instantiate(WorldController.instance.batteryRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

    public int DrainEnergy(int maxDrain)
    {
        int drained = Math.Min(energy, maxDrain);
        energy -= drained;
        return drained;
    }

}
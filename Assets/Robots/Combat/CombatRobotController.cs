using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombatRobotController : Robot
{

    // ********** SETTINGS **********

    public static int Settings_CopperCost = 5;
    public static int Settings_IronCost = 2;
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 0; }
    public override int Settings_Damage() { return 1; }
    public override int Settings_MaxHealth() { return 5; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.MeleeAttack
    };
    public override List<string> GetSpecializedInstruction() { return spezializedInstructions; }


    protected override void Animate()
    {
        return;
    }
}

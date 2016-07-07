using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public class CombatRobotController : RobotController
{
    public Animator animator;

    // ********** SETTINGS **********

    public static string Settings_Name = "PREDATOR";
    public static int Settings_CopperCost = 5;
    public static int Settings_IronCost = 2;
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 0; }
    public override int Settings_Damage() { return 1; }
    public override int Settings_StartHealth() { return 5; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.AttackMelee,
        Instructions.AttackUp,
        Instructions.AttackDown,
        Instructions.AttackLeft,
        Instructions.AttackRight
    };
    public override List<string> GetSpecializedInstruction() { return spezializedInstructions; }


    [Client]
    protected override void Animate()
    {
        string instruction = instructions.Count > 0 ? instructions[currentInstructionIndex] : string.Empty;
        if (new List<string>() { Instructions.AttackUp, Instructions.AttackLeft, Instructions.AttackRight, Instructions.AttackDown }.Any(instruction.Contains))
        {
            animator.Play("CombatRobotRangedAttack");
        }
        else if (instruction == Instructions.AttackMelee)
        {
            animator.Play("CombatRobotMeleeAttack");
        }
    }

    public override string GetName()
    {
        return Settings_Name;
    }
}

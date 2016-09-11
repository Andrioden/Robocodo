using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public class CombatRobotController : RobotController
{
    public Animator bodyAnimator;
    public Animator leftWeaponAnimator;
    public Animator rightWeaponAnimator;

    // ********** SETTINGS **********

    public static string Settings_name = "PREDATOR";
    public static int Settings_copperCost = 5;
    public static int Settings_ironCost = 2;

    public override string Settings_Name() { return Settings_name; }
    public override int Settings_CopperCost() { return Settings_copperCost; }
    public override int Settings_IronCost() { return Settings_ironCost; }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 0; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 1; }
    public override int Settings_StartHealth() { return 5; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.AttackMelee,
        Instructions.AttackUp,
        Instructions.AttackDown,
        Instructions.AttackLeft,
        Instructions.AttackRight,
        Instructions.AttackRandom
    };
    public override List<string> GetSpecializedInstruction() { return spezializedInstructions; }

    protected override List<string> GetDefaultInstructions()
    {
        return new List<string>()
        {
            Instructions.MoveUp,
            Instructions.MoveUp,
            Instructions.MoveRight,
            Instructions.MoveRight,
            Instructions.MoveRight,
            Instructions.MoveRight,
            Instructions.MoveDown,
            Instructions.MoveDown,
            Instructions.MoveHome,

            //Instructions.LoopStartNumberedSet(0, 5),
            //Instructions.MoveUp,
            //Instructions.LoopEnd
        };
    }

    protected override void Animate()
    {
        if (ShouldAnimationBePlayed())
        {
            string instruction = instructions.Count > 0 ? instructions[currentInstructionIndex] : string.Empty;

            if (new List<string>() { Instructions.AttackUp, Instructions.AttackLeft, Instructions.AttackRight, Instructions.AttackDown, Instructions.AttackRandom }.Any(instruction.Contains))
            {
                bodyAnimator.Play("Idle");
                leftWeaponAnimator.Play("Shoot");
                rightWeaponAnimator.Play("Shoot");
            }
            else if (instruction == Instructions.AttackMelee)
            {
                bodyAnimator.Play("Idle");
                leftWeaponAnimator.Play("Shoot");
                rightWeaponAnimator.Play("Shoot");
            }
            else if (new List<string>() { Instructions.MoveUp, Instructions.MoveDown, Instructions.MoveLeft, Instructions.MoveRight, Instructions.MoveHome, Instructions.MoveRandom }.Any(instruction.Contains))
            {
                bodyAnimator.Play("Walk");
            }
            else
                bodyAnimator.Play("Idle");
        }
        else
            bodyAnimator.Play("Idle");
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return (GameObject)Instantiate(WorldController.instance.combatRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }
}

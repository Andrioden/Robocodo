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
    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

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
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 1; }
    public override int Settings_StartHealth() { return 5; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        new Instruction_Attack(AttackDirection.Melee),
        new Instruction_Attack(AttackDirection.Up),
        new Instruction_Attack(AttackDirection.Down),
        new Instruction_Attack(AttackDirection.Left),
        new Instruction_Attack(AttackDirection.Right),
        new Instruction_Attack(AttackDirection.Random),

    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Down),
            new Instruction_Move(MoveDirection.Down),
            new Instruction_Move(MoveDirection.Home),
        };
    }

    protected override void Animate()
    {
        if (ShouldAnimationBePlayed())
        {
            if (LastAppliedInstruction.GetType() == typeof(Instruction_Attack))
            {
                bodyAnimator.Play("Idle");
                leftWeaponAnimator.Play("Shoot");
                rightWeaponAnimator.Play("Shoot");
            }
            else if (LastAppliedInstruction.GetType() == typeof(Instruction_Move))
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

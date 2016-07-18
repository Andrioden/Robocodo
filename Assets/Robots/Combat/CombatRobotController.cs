﻿using UnityEngine;
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

    public static string Settings_Name = "PREDATOR";
    public static int Settings_CopperCost = 5;
    public static int Settings_IronCost = 2;
    public override int Settings_Memory() { return 200; }
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

    public override string GetDemoInstructions()
    {
        List<string> demoInstructions = new List<string>()
        {
            Instructions.MoveUp,
            Instructions.AttackDown,
            Instructions.MoveHome,

            Instructions.MoveDown,
            Instructions.AttackUp,
            Instructions.MoveHome,

            Instructions.MoveLeft,
            Instructions.AttackRight,
            Instructions.MoveHome,

            Instructions.MoveRight,
            Instructions.AttackLeft,
            Instructions.MoveHome,
        };

        return string.Join("\n", demoInstructions.ToArray());
    }

    public override string GetName()
    {
        return Settings_Name;
    }

    protected override void Animate()
    {
        string instruction = instructions.Count > 0 ? instructions[currentInstructionIndex] : string.Empty;
        if (CurrentInstructionIndexIsValid)
        {
            if (new List<string>() { Instructions.AttackUp, Instructions.AttackLeft, Instructions.AttackRight, Instructions.AttackDown }.Any(instruction.Contains))
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
            else if (new List<string>() { Instructions.MoveUp, Instructions.MoveDown, Instructions.MoveLeft, Instructions.MoveRight, Instructions.MoveHome }.Any(instruction.Contains))
            {
                bodyAnimator.Play("Walk");
            }
            else
                bodyAnimator.Play("Idle");
        }
        else
            bodyAnimator.Play("Idle");
    }
}

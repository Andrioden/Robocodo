using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;
using System.Linq;

public class HarvesterRobotController : RobotController
{
    //public MeshRenderer visirMeshRenderer;
    public Animator bodyAnimator;
    public ParticleSystem leftToolParticleSystem;
    public ParticleSystem rightToolParticleSystem;


    // ********** SETTINGS **********

    public static string Settings_Name = "HARVESTER";
    public static int Settings_CopperCost = 1;
    public static int Settings_IronCost = 3;

    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 2; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.Harvest,
        Instructions.DropInventory
    };

    public override List<string> GetSpecializedInstruction() { return spezializedInstructions; }

    public override string GetDemoInstructions()
    {
        List<string> demoInstructions = new List<string>()
        {
            Instructions.MoveUp,
            Instructions.MoveUp,
            Instructions.MoveUp,
            Instructions.MoveUp,
            Instructions.MoveUp,
            Instructions.Harvest,
            Instructions.Harvest,
            Instructions.MoveHome,
            Instructions.DropInventory
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
            if (instruction == Instructions.Harvest)
            {
                bodyAnimator.Play("Idle");
                PlayHarvestParticleSystem();
            }
            else
                bodyAnimator.Play("Idle");
        }
        else
            bodyAnimator.Play("Idle");
    }

    private void PlayHarvestParticleSystem()
    {
        leftToolParticleSystem.Play();
        rightToolParticleSystem.Play();
    }
}
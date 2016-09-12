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

    public static string Settings_name = "HARVESTER";
    public static int Settings_copperCost = 1;
    public static int Settings_ironCost = 3;

    public override string Settings_Name() { return Settings_name; }
    public override int Settings_CopperCost() { return Settings_copperCost; }
    public override int Settings_IronCost() { return Settings_ironCost; }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 2; }
    public override int Settings_HarvestYield() { return 1; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.Harvest,
        Instructions.DropInventory
    };
    public override List<string> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<string> GetSuggestedInstructionSet()
    {
        return new List<string>()
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
    }

    protected override void Animate()
    {
        if (ShouldAnimationBePlayed())
        {
            if (lastAppliedInstruction == Instructions.Harvest)
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

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return (GameObject)Instantiate(WorldController.instance.harvesterRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

}
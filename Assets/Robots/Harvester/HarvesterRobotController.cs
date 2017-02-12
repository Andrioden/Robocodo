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
    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "HARVESTER";
    public static Cost Settings_cost() { return new Cost() { Copper = 1, Iron = 3 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.yellow; }
    public override Cost Settings_Cost() { return Settings_cost(); }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 2; }
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 1; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        new Instruction_Harvest(),
        new Instruction_DropInventory(),
    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Harvest(),
            new Instruction_Harvest(),
            new Instruction_Move(MoveDirection.Home),
            new Instruction_DropInventory(),
        };
    }

    protected override void Animate()
    {
        if (ShouldAnimationBePlayed())
        {
            if (LastAppliedInstruction.GetType() == typeof(Instruction_Harvest))
                PlayHarvestParticleSystem();
        }
        else if(energy <= 0)
            StartCoroutine(PlayDeactivateAnimation(1f));
    }

    private void PlayHarvestParticleSystem()
    {
        if (!leftToolParticleSystem.isPlaying || !rightToolParticleSystem.isPlaying)
        {
            leftToolParticleSystem.Play();
            rightToolParticleSystem.Play();
        }
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return (GameObject)Instantiate(WorldController.instance.harvesterRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

    IEnumerator PlayDeactivateAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        bodyAnimator.Play("Death");
        foreach (var light in GetComponentsInChildren<Light>())
            light.enabled = false;
    }

}
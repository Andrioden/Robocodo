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
    public ParticleSystem leftWeaponParticleSystem;
    public ParticleSystem rightWeaponParticleSystem;
    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "PREDATOR";
    public static Cost Settings_cost() { return new Cost() { Copper = 20, Iron = 12 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.red; }
    public override Cost Settings_Cost() { return Settings_cost(); }
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
        new Instruction_Attack(AttackType.Nearby3)
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
        if (!meshGO.activeSelf)
            return;

        if (ShouldAnimationBePlayed())
        {
            if (LastAppliedInstruction.GetType() == typeof(Instruction_Attack) && lastAttackedTargetWasAnHit)
            {
                bodyAnimator.Play("Idle");
                PlayShootingAnimation();
            }
            else if (LastAppliedInstruction.GetType() == typeof(Instruction_Move))
                bodyAnimator.Play("Walk");
            else
                bodyAnimator.Play("Idle");
        }
        else if (energy <= 0)        
            StartCoroutine(PlayDeactivateAnimation(1f)); //If we add a way to restart a robot that ran out of energy then we should animate Activate as well.        
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return Instantiate(WorldController.instance.combatRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

    private void PlayShootingAnimation()
    {
        if (!leftWeaponParticleSystem.isPlaying || !rightWeaponParticleSystem.isPlaying)
        {
            leftWeaponParticleSystem.Play();
            rightWeaponParticleSystem.Play();
        }

        leftWeaponAnimator.Play("Shoot");
        rightWeaponAnimator.Play("Shoot");
    }

    private IEnumerator PlayDeactivateAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        bodyAnimator.Play("Deactivate");
        foreach (var light in GetComponentsInChildren<Light>())
            light.enabled = false;
    }
}

using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Collections;
using System.Linq;

public class HarvesterRobotController : Robot, ISelectable
{
    public MeshRenderer visirMeshRenderer;
    public Animator animator;


    // ********** SETTINGS **********

    public static int Settings_CopperCost = 1;
    public static int Settings_IronCost = 3;
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 1; }
    public override int Settings_MaxEnergy() { return 50; }
    public override int Settings_InventoryCapacity() { return 2; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_MaxHealth() { return 1; }

    private List<string> spezializedInstructions = new List<string>()
    {
        Instructions.Harvest,
        Instructions.DropInventory
    };
    public override List<string> GetSpecializedInstruction() { return spezializedInstructions; }


    [Client]
    protected override void Animate()
    {
        string instruction = instructions.Count > 0 ? instructions[instructionBeingExecuted] : string.Empty;
        switch (instruction)
        {
            case Instructions.Harvest:
                animator.Play("HarvesterHarvest");
                break;

            default:
                animator.Play("HarvesterIdle");
                break;
        }
    }

    // TODO: Should be abstracted to Robot, but required a bit a of a rewrite of RobotPanel class, skipping for now.
    public void Click()
    {
        if (hasAuthority)
            RobotPanel.instance.ShowPanel(this);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        visirMeshRenderer.material.color = Color.blue;
    }

}
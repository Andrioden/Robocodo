using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransporterRobotController : RobotController
{

    public Sprite sprite;
    public override Sprite Sprite() { return sprite; }

    // ********** SETTINGS **********

    public static string Settings_name = "TRANSPORTER";
    public static Cost Settings_cost() { return new Cost() { Copper = 5, Iron = 20 }; }

    public override string Settings_Name() { return Settings_name; }
    public override Color Settings_Color() { return Color.blue; }
    public override Cost Settings_Cost() { return Settings_cost(); }
    public override int Settings_Memory() { return 20; }
    public override int Settings_IPT() { return 2; }
    public override int Settings_MaxEnergy() { return 200; }
    public override int Settings_InventoryCapacity() { return 10; }
    public override int Settings_ModuleCapacity() { return 1; }
    public override int Settings_HarvestYield() { return 0; }
    public override int Settings_Damage() { return 0; }
    public override int Settings_StartHealth() { return 1; }

    private List<Instruction> spezializedInstructions = new List<Instruction>()
    {
        new Instruction_OpenInventory(),
        new Instruction_DropInventory(),
        new Instruction_PickUp(),
        new Instruction_IdleUntilThen(UntilWhat.Full, null),
    };
    public override List<Instruction> GetSpecializedInstructions() { return spezializedInstructions; }

    protected override List<Instruction> GetSuggestedInstructionSet()
    {
        return new List<Instruction>()
        {
            new Instruction_Idle(),
            new Instruction_Move(MoveDirection.Up),
        };
    }

    protected override void Animate()
    {
        if (!meshGO.activeSelf)
            return;

        if (energy <= 0)
            StartCoroutine(PlayDeactivateAnimation(1f));
    }

    public override GameObject SpawnPreviewGameObjectClone()
    {
        return Instantiate(WorldController.instance.harvesterRobotPrefab, new Vector3(x, 1, z), Quaternion.identity);
    }

    IEnumerator PlayDeactivateAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var light in GetComponentsInChildren<Light>())
            light.enabled = false;
    }

}
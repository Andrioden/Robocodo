using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SolarPanelModule : Module
{
    public static readonly string SerializedType = "SolarPanelModule";

    public override string Serialize()
    {
        return SerializedType;
    }

    // ********** SETTINGS **********
    private int energyPerTick = 1;

    public override string Settings_Name() { return "Solar Panel"; }
    public override string Settings_Description() { return string.Format("Generates {0} energy per tick.", energyPerTick); }
    public override int Settings_CopperCost() { return 50; }
    public override int Settings_IronCost() { return 50; }

    protected override void Tick(object sender)
    {
        robot.AddEnergy(energyPerTick);
    }

    protected override void HalfTick(object sender)
    {

    }

    public override List<Instruction> GetInstructions()
    {
        return new List<Instruction>();
    }
}

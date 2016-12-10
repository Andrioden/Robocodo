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
    public static Cost Settings_cost() { return new Cost() { Copper = 5, Iron = 20 }; }

    public override string Settings_Name() { return "Solar Panel"; }
    public override string Settings_Description() { return string.Format("Generates {0} energy per tick.", energyPerTick); }
    public override Cost Settings_Cost() { return Settings_cost(); }

    protected override void Tick()
    {
        robot.AddEnergy(energyPerTick);
    }

    protected override void HalfTick()
    {

    }

    public override List<Instruction> GetInstructions()
    {
        return new List<Instruction>();
    }
}

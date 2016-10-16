using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SolarPanelModule : Module
{
    public static readonly string SerializedType = "SolarPanelModule";

    private string name = "Solar Panel";
    private string description = "Generates {0} energy per tick.";
    private int energyPerTick = 1;

    public override string Serialize()
    {
        return SerializedType;
    }

    public override string Settings_Name()
    {
        return name;
    }

    public override string Settings_Description()
    {
        return string.Format(description, energyPerTick);
    }

    protected override void Tick(object sender)
    {
        robot.AddEnergy(energyPerTick);
    }

    protected override void HalfTick(object sender)
    {

    }

    public override List<string> GetInstructions()
    {
        return new List<string>();
    }
}

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

    protected override void Tick(object sender)
    {
        robot.AddEnergy(1);
    }

    protected override void HalfTick(object sender)
    {

    }

    public override List<string> GetInstructions()
    {
        return new List<string>();
    }
}

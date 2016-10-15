using UnityEngine;
using System.Collections.Generic;

public abstract class Module
{
    public abstract string Serialize();
    protected RobotController robot;

    protected abstract void Tick(object sender);
    protected abstract void HalfTick(object sender);
    public abstract List<string> GetInstructions();

    public void Install(RobotController robot)
    {
        this.robot = robot;
        WorldTickController.instance.TickEvent += Tick;
        WorldTickController.instance.HalfTickEvent += HalfTick;
        robot.CacheAllowedInstructions();
    }

    public void Uninstall(bool recacheInstructions = true)
    {
        WorldTickController.instance.TickEvent -= Tick;
        WorldTickController.instance.HalfTickEvent -= HalfTick;
        if (recacheInstructions)
            robot.CacheAllowedInstructions();
        robot = null;
    }
}

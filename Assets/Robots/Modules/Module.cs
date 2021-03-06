﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public abstract class Module
{
    public abstract string Serialize();
    public abstract string Settings_Name();
    public abstract string Settings_Description();
    public abstract Cost Settings_Cost();

    protected RobotController robot;

    protected abstract void Tick();
    protected abstract void HalfTick();
    public abstract List<Instruction> GetInstructions();

    public void Install(RobotController robot)
    {
        this.robot = robot;
        WorldTickController.instance.OnTick += Tick;
        WorldTickController.instance.OnHalfTick += HalfTick;
        robot.CacheAllowedInstructions();
    }

    public void Uninstall(bool cacheInstructions = true)
    {
        if (robot == null)
            Debug.LogError("Tried to uninstall a module without a robot, most likely on a client, should only be uninstalled on server.");

        WorldTickController.instance.OnTick -= Tick;
        WorldTickController.instance.OnHalfTick -= HalfTick;
        if (cacheInstructions)
            robot.CacheAllowedInstructions();
        robot = null;
    }

    public static string[] SerializeList(List<Module> modules)
    {
        return modules.Select(m => m.Serialize()).ToArray();
    }

    public static List<Module> DeserializeList(string[] serializedModules)
    {
        List<Module> modules = new List<Module>();

        foreach (string serializedModule in serializedModules)
            modules.Add(Deserialize(serializedModule));

        return modules;
    }

    public static Module Deserialize(string serializedModule)
    {
        if (serializedModule == SolarPanelModule.SerializedType)
            return new SolarPanelModule();
        else
            throw new Exception("Not added support for the serialized module: " + serializedModule);
    }
}

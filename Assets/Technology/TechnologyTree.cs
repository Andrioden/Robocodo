using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class TechnologyTree
{

    private int techIdIterator = 0;

    public List<Technology> Technologies = new List<Technology>();

    public TechnologyTree()
    {
        Technologies.Add(new Technology_Robot(techIdIterator++, 100, HarvesterRobotController.Settings_name, true));
        Technologies.Add(new Technology_Robot(techIdIterator++, 100, CombatRobotController.Settings_name, true));
        Technologies.Add(new Technology_Robot(techIdIterator++, 100, TransporterRobotController.Settings_name, false));
        Technologies.Add(new Technology_Robot(techIdIterator++, 100, PurgeRobotController.Settings_name, false));
    }
    
    public bool IsRobotUnlocked(string robotName)
    {
        foreach(Technology_Robot tech in Technologies.Where(t => t is Technology_Robot))
        {
            if (tech.RobotName == robotName)
                return tech.IsResearched();
        }

        return true;
        //throw new Exception("Tried to ask if an robot was unlocked for a robot not in the tech tree: " + robotName);
    }

}

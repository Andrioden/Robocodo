using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Technology_Robot : Technology
{

    public string RobotName;

    public Technology_Robot(int id, int scienceCost, string robotName, bool researched)
    {
        Id = id;
        ScienceCost = scienceCost;
        this.RobotName = robotName;

        if (researched)
            ScienceProgress = ScienceCost;
    }

}
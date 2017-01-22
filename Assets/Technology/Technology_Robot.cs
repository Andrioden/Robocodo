using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Technology_Robot : Technology
{

    public string robotName;

    public Technology_Robot(TechnologyTree techTree, int id, string name, int scienceCost, string robotName) : base(techTree, id, name, scienceCost)
    {
        this.robotName = robotName;
    }

    public override void Complete()
    {
        techTree.robotsThatCanBeBuilt.Add(robotName);
    }

}
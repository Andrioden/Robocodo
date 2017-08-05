using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Technology_Robot : Technology
{

    public Type robotType;

    public Technology_Robot(TechnologyTree techTree, int id, string name, string description, int scienceCost, Type robotType) : base(techTree, id, name, description, scienceCost)
    {
        this.robotType = robotType;
    }

    public override void Complete()
    {
        base.Complete();
        techTree.TriggerNewRobotResearchedEvent();
    }
}
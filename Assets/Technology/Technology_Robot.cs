using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Technology_Robot : Technology
{

    public Type robotType;

    public Technology_Robot(TechnologyTree techTree, int id, string name, int scienceCost, Type robotType) : base(techTree, id, name, scienceCost)
    {
        this.robotType = robotType;
    }

}
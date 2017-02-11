using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Technology_Victory : Technology
{
    public Technology_Victory(TechnologyTree techTree, int id, string name, int cost) : base(techTree, id, name, cost) { }

    public override void Complete() { }

}
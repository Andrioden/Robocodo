using System.Collections;
using System.Collections.Generic;

public abstract class Technology
{

    public int Id;
    public int ScienceProgress;
    public int ScienceCost;

    public bool IsResearched()
    {
        return ScienceProgress >= ScienceCost;
    }

    public string Serialize()
    {
        return Id + "_" + ScienceProgress;
    }

}

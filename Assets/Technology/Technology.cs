using System.Collections;
using System.Collections.Generic;
using System;

public abstract class Technology
{

    public TechnologyTree techTree;

    public int id;
    public string name;

    private int progress = 0;
    public int Progress { get { return progress; } }

    public int cost;

    public event Action OnNewRobotResearched = delegate { };

    public Technology(TechnologyTree techTree, int id, string name, int cost)
    {
        this.techTree = techTree;
        this.id = id;
        this.name = name;
        this.cost = cost;
    }

    public void SetProgress(int newProgress)
    {
        progress = Math.Min(cost, newProgress);

        if (progress >= cost)
            Complete();
    }

    public void AddProgress(int addedProgress)
    {
        progress = Math.Min(cost, progress + addedProgress);

        if (progress >= cost)
            Complete();
    }

    public bool IsResearched()
    {
        return progress >= cost;
    }

    public virtual void Complete() { }
}

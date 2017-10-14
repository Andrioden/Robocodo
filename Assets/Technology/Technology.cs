using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public abstract class Technology
{

    public TechnologyTree techTree;

    public int id;
    public string name;
    public string description;

    private int progress = 0;
    public int Progress { get { return progress; } }

    public int cost;

    public Technology(TechnologyTree techTree, int id, string name, string description, int cost)
    {
        this.techTree = techTree;
        this.id = id;
        this.name = name;
        this.description = description;
        this.cost = cost;
    }

    public void AddProgress(int addedProgress)
    {
        int newProgress = Math.Min(cost, progress + addedProgress);
        SetProgress(newProgress);
    }

    public void SetProgress(int newProgress)
    {
        if (IsResearched())
            return;

        progress = Math.Min(cost, newProgress);

        if (IsResearched())
            Complete();
    }

    public int GetProgressPercent()
    {
        return (int)Math.Floor(100.0 * progress / cost);
    }

    public bool IsResearched()
    {
        return progress >= cost;
    }

    public virtual void Complete() { }
}

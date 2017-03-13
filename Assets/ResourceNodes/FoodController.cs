using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : ResourceController
{

    private double growthPerTick;
    private double accumulatedGrowth = 0;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        growthPerTick = Utils.RandomDouble(Settings.Resource_Food_GrowthPerTick_Min, Settings.Resource_Food_GrowthPerTick_Max);

        WorldTickController.instance.OnTick += Grow;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= Grow;
    }

    private void Grow()
    {
        accumulatedGrowth += growthPerTick;
        if (accumulatedGrowth >= 1)
        {
            int growth = (int)accumulatedGrowth; //Floors it
            remainingItems = Mathf.Min(Settings.Resource_ItemsPerNode_Max, remainingItems + growth);
            accumulatedGrowth -= growth;

            UpdateTransformSize();
        }
    }

}

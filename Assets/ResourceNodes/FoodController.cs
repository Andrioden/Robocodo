using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodController : ResourceController
{

    private double growthPerTickAverage;
    private double accumulatedGrowth = 0;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        growthPerTickAverage = Utils.RandomDouble(0.1, 2.0);

        WorldTickController.instance.OnTick += Grow;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= Grow;
    }

    private void Grow()
    {
        accumulatedGrowth += growthPerTickAverage;
        if (accumulatedGrowth >= 1)
        {
            int growth = (int)accumulatedGrowth; //Floors it
            remainingItems = Mathf.Min(Settings.Resource_MaxItemsPerNode, remainingItems + growth);
            accumulatedGrowth -= growth;

            UpdateTransformSize();
        }
    }

}

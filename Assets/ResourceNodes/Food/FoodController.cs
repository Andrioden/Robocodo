using UnityEngine;

public class FoodController : ResourceController
{

    private double growthPerTick;
    private double accumulatedGrowth = 0;

    public override string SerializedInventoryType() { return FoodItem.SerializedType; }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        growthPerTick = Utils.RandomDouble(Settings.World_Gen_FoodGrowthPerTick_Min, Settings.World_Gen_FoodGrowthPerTick_Max);

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
            remainingItems = Mathf.Min(Settings.World_Gen_ResourceItemsPerNode_Max, remainingItems + growth);
            accumulatedGrowth -= growth;

            UpdateTransformSize();
        }
    }

    public override string GetSummary()
    {
        return base.GetSummary() + string.Format("\nGrowth: {0:0.00}", growthPerTick);
    }
}

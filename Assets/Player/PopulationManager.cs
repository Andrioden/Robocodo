using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public class PopulationManager : NetworkBehaviour
{

    private CityController city;

    [SyncVar]
    private int population;
    public int Population { get { return population; } }

    private double foodConsumeProgress = 0;

    [SyncVar]
    private double growthProgress = 0;
    public double GrowthProgress { get { return growthProgress; } }

    private ValueTracker foodValueTracker;
    [SyncVar]
    private double foodChangePerTick = 0;
    public double FoodChangePerTick { get { return foodChangePerTick; } }

    [Server]
    public void Initialize(CityController city)
    {
        this.city = city;
        population = 1;
        foodValueTracker = new ValueTracker(0.1, city.GetItemCount<FoodItem>());

        WorldTickController.instance.OnTick += ConsumeFoodAndGrow;
        WorldTickController.instance.OnAfterTick += CalculateFoodPerTick;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= ConsumeFoodAndGrow;
        WorldTickController.instance.OnAfterTick -= CalculateFoodPerTick;
    }

    [Server]
    private void ConsumeFoodAndGrow()
    {
        ConsumeFood();
        Grow();
    }

    [Server]
    private void ConsumeFood()
    {
        if (population <= 0)
            return;

        foodConsumeProgress += Settings.City_Population_FoodConsumedPerTick * population;
        if (foodConsumeProgress >= 1)
        {
            Cost foodConsumed = new Cost() { Food = (int)foodConsumeProgress };
            if (city.CanAfford(foodConsumed))
            {
                city.RemoveResources(foodConsumed);
                foodConsumeProgress -= foodConsumed.Food;
            }
            else
            {
                growthProgress = 0;
                foodConsumeProgress = 0;
                population--;

                if (population <= 0)
                    return;
            }
        }
    }
    [Server]
    private void Grow()
    {
        if (population <= 0)
            return;

        // Algorithm #1 - Exponential increased difficulty growing
        //growthProgress += Settings.City_Population_FoodConsumedPerTick * city.GetItemCount<FoodItem>() / (population * population);

        // Algorithm #2 - Linearly*10 increased difficulty growing
        growthProgress += Settings.City_Population_FoodConsumedPerTick * Math.Max(0, (city.GetItemCount<FoodItem>() - (population * 10.0)));

        if (growthProgress >= 1)
        {
            population += (int)growthProgress;
            growthProgress -= (int)growthProgress;
        }
    }

    [Server]
    private void CalculateFoodPerTick()
    {
        foodValueTracker.AddDataPoint(WorldTickController.instance.Tick, city.GetItemCount<FoodItem>());
        foodChangePerTick = foodValueTracker.ChangePerTick;
    }

}
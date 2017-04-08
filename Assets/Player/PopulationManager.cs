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

        WorldTickController.instance.OnTick += Grow;
        WorldTickController.instance.OnAfterTick += CalculateFoodPerTick;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= Grow;
        WorldTickController.instance.OnAfterTick -= CalculateFoodPerTick;
    }

    [Server]
    private void Grow()
    {
        if (population <= 0)
            return;

        growthProgress += Settings.City_Population_FoodConsumedPerTick * population;
        if (growthProgress >= 1)
        {
            int foodGrowthCost = (int)growthProgress;
            Cost growthCost = new Cost() { Food = foodGrowthCost };

            if (city.CanAfford(growthCost))
            {
                city.RemoveResources(growthCost);
                growthProgress -= growthProgress;
                population++;
            }
            else
            {
                growthProgress = 0;
                population--;
            }
        }
    }

    [Server]
    private void CalculateFoodPerTick()
    {
        foodValueTracker.AddDataPoint(WorldTickController.instance.Tick, city.GetItemCount<FoodItem>());
        foodChangePerTick = foodValueTracker.ChangePerTick;
    }

}
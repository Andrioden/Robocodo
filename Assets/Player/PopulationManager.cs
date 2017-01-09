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

    [Server]
    public void Initialize(CityController city)
    {
        this.city = city;
        population = 1;

        WorldTickController.instance.OnTick += Grow;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= Grow;
    }

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

}


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robocodo;

namespace Robocodo.Tests
{
    [TestClass]
    public class BalanceTests
    {
        [TestMethod]
        public void BalanceAnalyze()
        {
            Console.WriteLine(" --- TIME ---");
            Console.WriteLine("- 1 tick is {0}seconds", Settings.World_Time_IrlSecondsPerTick);

            Console.WriteLine("");
            Console.WriteLine(" --- RESOURCES ---");

            int startingAreaCopper = (Settings.World_Gen_ResourceItemsPerNode_Min + Settings.World_Gen_ResourceItemsPerNode_Max) * Settings.World_Gen_PlayerStartingAreaCopper / 2;
            int startingAreaIron = (Settings.World_Gen_ResourceItemsPerNode_Min + Settings.World_Gen_ResourceItemsPerNode_Max) * Settings.World_Gen_PlayerStartingAreaIron / 2;
            int startingAreaFood = (Settings.World_Gen_ResourceItemsPerNode_Min + Settings.World_Gen_ResourceItemsPerNode_Max) * Settings.World_Gen_PlayerStartingAreaFood / 2;
            double foodGrowdPerTick = (Settings.World_Gen_FoodGrowthPerTick_Min + Settings.World_Gen_FoodGrowthPerTick_Max) * Settings.World_Gen_PlayerStartingAreaFood / 2;
            int startingAreaFoodAfter100ticks = startingAreaFood + (int)(100 * foodGrowdPerTick);

            double ticksPerResourceGatheredByHarvester =
                (                                                       // Total Instructions
                    (                                                       
                        ((1.0 + Settings.World_Gen_PlayerStartingAreaResourceRadius) / 2) // Average distance
                        * 2                                                 // Has to go two ways
                        * 2                                                 // Distance is randomed in two directions
                    )                                                    
                    + HarvesterRobotController.Settings_inventoryCapacity   // 1 instruction per harvest
                    + 1                                                     // Drop inventory
                )                                                    
                / HarvesterRobotController.Settings_inventoryCapacity;  // Harvested                                                           

            Console.WriteLine("- Starting Area Copper: {0}", startingAreaCopper);
            Console.WriteLine("- Starting Area Iron: {0}", startingAreaIron);
            Console.WriteLine("- Starting Area Food: {0}, with {1} food availible after 100 ticks", startingAreaFood, startingAreaFoodAfter100ticks);
            Console.WriteLine("- Harvesters has to spend {0} ticks to gather 1 resource", ticksPerResourceGatheredByHarvester);

            Console.WriteLine("");
            Console.WriteLine(" --- FOOD SURVIVAL ---");
            Console.WriteLine("- Each population consume {0} food per tick", Settings.City_Population_FoodConsumedPerTick);
            SimulateHowLongAreaFoodLasts(50, 0, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
            SimulateHowLongAreaFoodLasts(50, 1, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
            SimulateHowLongAreaFoodLasts(50, 2, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
            SimulateHowLongAreaFoodLasts(50, 3, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
            SimulateHowLongAreaFoodLasts(50, 4, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
            SimulateHowLongAreaFoodLasts(50, 5, startingAreaFood, foodGrowdPerTick, ticksPerResourceGatheredByHarvester);
        }

        private void SimulateHowLongAreaFoodLasts(int startingFood, int harvesters, double startingAreaFood, double foodGrowdPerTick, double ticksPerResourceGatheredByHarvester)
        {
            int maxStartingAreaFood = Settings.World_Gen_PlayerStartingAreaFood * Settings.World_Gen_ResourceItemsPerNode_Max;

            int pop = 1;
            double food = startingFood;

            int maxTicks = 10000;

            int ticksPassed = 0;
            while (ticksPassed < maxTicks)
            {
                int ticksPassedThisSimulationRound = (int)Math.Ceiling(1.0 / (Settings.City_Population_FoodConsumedPerTick * pop));
                ticksPassed += ticksPassedThisSimulationRound;

                // Growth
                startingAreaFood = Math.Min(maxStartingAreaFood, startingAreaFood + (ticksPassedThisSimulationRound * foodGrowdPerTick));

                // Harvest
                double potentialFoodHarvest = (ticksPassedThisSimulationRound / ticksPerResourceGatheredByHarvester) * harvesters;
                food += Math.Min(startingAreaFood, potentialFoodHarvest);
                startingAreaFood -= potentialFoodHarvest;

                // Population and Consumation of Food
                if (food > 0)
                {
                    pop++;
                    food--; // eaten every time population grow, which it does every simulation/while loop
                }
                else
                    pop--;

                if (pop == 0)
                {
                    Console.WriteLine("- Per 21.05.2017 implementation. Food will last ca {0} ticks given {1} harvesters active all the time and {2} starting food.", ticksPassed, harvesters, startingFood);
                    return;
                }
            }

            Console.WriteLine("- Per 21.05.2017 implementation. Food will last ca {0} ticks given {1} harvesters active all the time and {2} starting food.", "endless", harvesters, startingFood);
            return;
        }
    }
}

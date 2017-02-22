public static class Settings
{
    public static float World_IrlSecondsPerTick = 1.0f;
    public static float World_MaxTimeScale = 20;

    //public static float World_CopperNoiseThreshold = 0.0f;
    //public static float World_CopperNoiseThreshold = 0.92f;
    //public static float World_IronNoiseThreshold = 0.92f;
    //public static float World_FoodNoiseThreshold = 0.92f;

    public static float World_CopperNoiseRangeFrom = 0.00f;
    public static float World_CopperNoiseRangeTo = 0.10f;

    public static float World_IronNoiseRangeFrom = 0.30f;
    public static float World_IronNoiseRangeTo = 0.33f;

    public static float World_FoodNoiseRangeFrom = 0.90f;
    public static float World_FoodNoiseRangeTo = 1.00f;

    public static int World_Infection_SpreadThreshold = 5;
    public static int World_Infection_SpreadDistance = 2;
    public static double World_Infection_GrowthPerTickPerTilePer100Infection = 0.13;
    public static double World_Infection_MaxInfectionImpactPerTile = 1.3;
    public static int World_Infection_InfectionImpactLoss = 100;

    public static float World_ScavengerMaxCount = 10;
    public static float World_ScavengerSpawnInterval = 300; // every x seconds on normal game speed
    public static int World_ScavengerAggressiveness = 1;

    public static double City_Population_FoodConsumedPerTick = 0.01;
    public static int City_Energy_RechargedPerTick = 500;
    public static double City_Science_PerPopulationPerTick = 1.0;

    public static int Resource_MinItemsPerNode = 100;
    public static int Resource_MaxItemsPerNode = 200;

    public static int Scenario_Normal_AmountOfStartingHarvesterRobots = 2;
    public static int Scenario_Normal_AmountOfStartingCombatRobots = 1;
    public static int Scenario_Normal_AmountOfStartingCopper = 50;
    public static int Scenario_Normal_AmountOfStartingIron = 50;
    public static int Scenario_Normal_AmountOfStartingFood = 50;

    public static int Robot_SalvagePercentage = 25;
    public static int Robot_ReprogramClearEachInstructionTicks = 5;

    public static int Robot_Purge_InfectionReducedPerTick = 10;

    public static bool GUI_EnableEdgeScrolling = false;
}
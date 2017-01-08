public static class Settings
{
    public static float World_IrlSecondsPerTick = 1.0f;
    public static float World_MaxTimeScale = 20;

    public static int World_Infection_SpreadDistance = 1;
    public static int World_Infection_GrowthPerTickPerTile = 1;
    public static double World_Infection_MaxInfectionLossImpactPerTile = 1.3;

    public static float World_ScavengerMaxCount = 10;
    public static float World_ScavengerSpawnInterval = 30; // every x seconds on normal game speed
    public static int World_ScavengerAggressiveness = 1;

    public static double City_Population_FoodConsumedPerTick = 0.01;
    public static int City_Energy_RechargedPerTick = 500;

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
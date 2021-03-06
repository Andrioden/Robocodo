﻿public static class Settings
{
    public static float World_Time_IrlSecondsPerTick = 1.0f;
    public static float World_Time_MaxTimeScale = 50;

    public static float World_Gen_CopperNoiseRangeFrom = 0.00f;
    public static float World_Gen_CopperNoiseRangeTo = 0.10f;
    public static float World_Gen_IronNoiseRangeFrom = 0.30f;
    public static float World_Gen_IronNoiseRangeTo = 0.33f;
    public static float World_Gen_FoodNoiseRangeFrom = 0.90f;
    public static float World_Gen_FoodNoiseRangeTo = 1.00f;

    public static int World_Gen_PlayerAreaRadius = 10;
    public static int World_Gen_PlayerStartingAreaResourceRadius = 5;
    public static int World_Gen_PlayerStartingAreaCopper = 3;
    public static int World_Gen_PlayerStartingAreaIron = 3;
    public static int World_Gen_PlayerStartingAreaFood = 3;

    public static int World_Gen_ResourceItemsPerNode_Min = 100;
    public static int World_Gen_ResourceItemsPerNode_Max = 200;
    public static double World_Gen_FoodGrowthPerTick_Min = 0.1;
    public static double World_Gen_FoodGrowthPerTick_Max = 1.0;

    public static int World_Infection_SpreadDistance = 1;
    public static double World_Infection_GrowthPerTickPerTile = 0.12;
    public static double World_Infection_MaxCityInfectionImpactPerTile = 1.3;
    public static int World_Infection_InfectionImpactLoss = 100;

    public static float World_ScavengerMaxCount = 10;
    public static float World_ScavengerSpawnInterval = 300; // every x seconds on normal game speed
    public static int World_ScavengerAggressiveness = 1;

    public static double City_Population_FoodConsumedPerTick = 0.01;
    public static double City_Science_PerPopulationPerTick = 1.0;
    public static int City_Energy_RechargedPerTick = 20; // Basically how many robots it can support

    public static int Scenario_Normal_AmountOfStartingHarvesterRobots = 2;
    public static int Scenario_Normal_AmountOfStartingCombatRobots = 0;
    public static int Scenario_Normal_AmountOfStartingCopper = 5;
    public static int Scenario_Normal_AmountOfStartingIron = 10;
    public static int Scenario_Normal_AmountOfStartingFood = 10;

    public static int Robot_SalvagePercentage = 25;
    public static int Robot_ReprogramClearEachInstructionTicks = 5;
    public static int Robot_PurgeInfectionReducedPerTick = 10;

    public static bool GUI_EnableEdgeScrolling = false;
    public static bool GUI_ShowFPSCounter = true;

    public static bool Debug_EnableGameLobby = false;
    public static bool Debug_EnableAiLogging = false;
    public static bool Debug_PlayerAsAI = false;

    public static bool Sound_EnableMusic = true;
}
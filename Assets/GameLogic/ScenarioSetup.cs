using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

public static class ScenarioSetup
{

    private static WorldController wc;

    private static List<ScenarioEntry> scenarios = new List<ScenarioEntry>();
    public static List<ScenarioEntry> Scenarios { get { return scenarios; } }

    public static bool IsAllTechUnlocked = false;

    static ScenarioSetup()
    {
        scenarios.Add(new ScenarioEntry("Normal", Scenario.Normal, Normal));
        scenarios.Add(new ScenarioEntry("Rich And Teched", Scenario.RichAndTeched, RichAndTeched));
        scenarios.Add(new ScenarioEntry("Wild PvE", Scenario.WildPvE, WildPvE));
        scenarios.Add(new ScenarioEntry("Test Attack Neu Enemy", Scenario.Test_AttackNeutralEnemy, Test_AttackNeutralEnemy));
        scenarios.Add(new ScenarioEntry("Test Logistics Chain", Scenario.Test_LogisticsChain, Test_LogisticsChain));
        scenarios.Add(new ScenarioEntry("Test Stacking Robots", Scenario.Test_StackingRobots, Test_StackingRobots));
        scenarios.Add(new ScenarioEntry("Test Battery Robot", Scenario.Test_BatteryRobot, Test_BatteryRobot));
        scenarios.Add(new ScenarioEntry("Test Infection Purging", Scenario.Test_InfectionPurge, Test_InfectionPurge));
        scenarios.Add(new ScenarioEntry("Test Infection Victory", Scenario.Test_InfectionVictory, Test_InfectionVictory));
        scenarios.Add(new ScenarioEntry("Test Crash", Scenario.Test_Crash, Test_Crash));
        scenarios.Add(new ScenarioEntry("Test Loop", Scenario.Test_Loop, Test_Loop));
    }

    public static void RegisterWorldController(WorldController worldController)
    {
        wc = worldController;
    }

    //public static void Run(int scenarioIndex, NetworkConnection conn, GameObject playerGO)
    //{
    //    scenarios[scenarioIndex].Run(conn, playerGO);
    //}

    /// <summary>
    /// NetworkConnection is null if AI
    /// </summary>
    public static void Run(Scenario scenario, NetworkConnection conn, PlayerController player)
    {
        IsAllTechUnlocked = false;

        ScenarioEntry scenarioEntry = scenarios.Where(s => s.ScenarioEnumRef == scenario).FirstOrDefault();
        if (scenarioEntry == null)
            throw new Exception("Not added support for the Scenario enum: " + scenario);
        else
            scenarioEntry.Run(conn, player);
    }

    private static void Normal(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddCopper(player.City, Settings.Scenario_Normal_AmountOfStartingCopper);
        AddIron(player.City, Settings.Scenario_Normal_AmountOfStartingIron);
        AddFood(player.City, Settings.Scenario_Normal_AmountOfStartingFood);

        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingHarvesterRobots; i++)
            AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X, player.City.Z, player, conn);
        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingCombatRobots; i++)
            AddRobot(WorldController.instance.combatRobotPrefab, player.City.X, player.City.Z, player, conn);
    }

    private static void RichAndTeched(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddCopper(player.City, 1000);
        AddIron(player.City, 1000);
        AddFood(player.City, 1000);

        IsAllTechUnlocked = true; // Unable to add this directly because the TechTree is created after this code runs
    }

    private static void WildPvE(NetworkConnection conn, PlayerController player)
    {
        Normal(conn, player);

        for (int x = 0; x < 1; x++)
        {
            GameObject combaRobotGO = wc.SpawnObject(wc.combatRobotPrefab, x, 0);
            CombatRobotController combatRobot = combaRobotGO.GetComponent<CombatRobotController>();

            combatRobot.SetInstructions(new List<Instruction>
            {
                new Instruction_Move(MoveDirection.Down),
                new Instruction_Move(MoveDirection.Down),
                new Instruction_Attack(AttackType.Nearby3)
            });
            combatRobot.AddModule(new SolarPanelModule());

            combatRobot.CmdStartRobot();
        }
    }

    private static void Test_AttackNeutralEnemy(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddFood(player.City, 20);

        wc.SpawnObject(wc.combatRobotPrefab, player.City.X + 2, player.City.Z);

        AddRobot(WorldController.instance.combatRobotPrefab, player.City.X, player.City.Z, player, conn, new List<Instruction>
        {
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Up),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Up),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Up),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Right),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Right),
            new Instruction_Attack(AttackType.Nearby3),

            new Instruction_Move(MoveDirection.Down),
            new Instruction_Attack(AttackType.Nearby3),
        });
    }

    private static void Test_LogisticsChain(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddFood(player.City, 20);

        wc.SpawnResourceNode(new CopperItem(), player.City.X + 4, player.City.Z);

        AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X + 4, player.City.Z, player, conn, new List<Instruction>
        {
            new Instruction_Harvest(),
            new Instruction_Move(MoveDirection.Left),
            new Instruction_DropInventory(),
            new Instruction_Move(MoveDirection.Right),
        });

        AddRobot(WorldController.instance.storageRobotPrefab, player.City.X + 3, player.City.Z, player, conn, new List<Instruction>
        {
            new Instruction_OpenInventory()
        });

        AddRobot(WorldController.instance.transporterRobotPrefab, player.City.X + 3, player.City.Z, player, conn, new List<Instruction>
        {
            new Instruction_PickUp(),
            new Instruction_Move(MoveDirection.Left),
            new Instruction_Move(MoveDirection.Left),
            new Instruction_Move(MoveDirection.Left),
            new Instruction_DropInventory(),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Move(MoveDirection.Right),
        });
    }

    private static void Test_StackingRobots(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddFood(player.City, 20);

        AddRobot(WorldController.instance.combatRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.combatRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.combatRobotPrefab, player.City.X + 2, player.City.Z, player, conn);

        AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);

        AddRobot(WorldController.instance.transporterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.transporterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);
        AddRobot(WorldController.instance.transporterRobotPrefab, player.City.X + 2, player.City.Z, player, conn);

        AddRobot(WorldController.instance.storageRobotPrefab, player.City.X + 2, player.City.Z, player, conn, new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Down)
        });

        AddRobot(WorldController.instance.storageRobotPrefab, player.City.X + 2, player.City.Z, player, conn, new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Left),
            new Instruction_Move(MoveDirection.Right)
        });

        AddRobot(WorldController.instance.storageRobotPrefab, player.City.X + 2, player.City.Z, player, conn, new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Down),
            new Instruction_Move(MoveDirection.Up)
        });
    }

    private static void Test_BatteryRobot(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddFood(player.City, 20);

        AddRobot(WorldController.instance.batteryRobotPrefab, player.City.X + 2, player.City.Z, player, conn);

        AddRobot(WorldController.instance.transporterRobotPrefab, player.City.X + 2, player.City.Z, player, conn, new List<Instruction>()
        {
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Up),
            new Instruction_Move(MoveDirection.Down),
            new Instruction_Move(MoveDirection.Down),
            new Instruction_Move(MoveDirection.Down),
        });
    }

    private static void Test_InfectionPurge(NetworkConnection conn, PlayerController player)
    {
        AddFood(player.City, 20);

        AddRobot(WorldController.instance.purgeRobotPrefab, player.City.X, player.City.Z, player, conn);
        AddRobot(WorldController.instance.purgeRobotPrefab, player.City.X, player.City.Z, player, conn);
        AddRobot(WorldController.instance.purgeRobotPrefab, player.City.X, player.City.Z, player, conn);

        foreach (Coordinate coord in wc.worldBuilder.GetCoordinatesNear(player.City.X, player.City.Z, 1))
            InfectionManager.instance.IncreaseTileInfection(coord.x, coord.z, 20);
    }

    private static void Test_InfectionVictory(NetworkConnection conn, PlayerController player)
    {
        AddFood(player.City, 20);

        AddRobot(WorldController.instance.purgeRobotPrefab, player.City.X + 1, player.City.Z, player, conn, new List<Instruction>()
        {
            new Instruction_Purge()
        });

        InfectionManager.instance.IncreaseTileInfection(player.City.X + 1, player.City.Z, 20);
    }

    private static void Test_Crash(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        var robotPrefabs = new List<GameObject>() {
            WorldController.instance.harvesterRobotPrefab,
            WorldController.instance.combatRobotPrefab,
            WorldController.instance.transporterRobotPrefab,
            WorldController.instance.storageRobotPrefab,
            WorldController.instance.batteryRobotPrefab,
            WorldController.instance.purgeRobotPrefab
        };

        int xOffset = 1;
        foreach (var robotType in robotPrefabs)
        {
            foreach (var oppositRobotPrefab in robotPrefabs)
            {
                AddRobot(oppositRobotPrefab, xOffset, 6, player, conn, new List<Instruction>()
                {
                    new Instruction_Move(MoveDirection.Down)
                });

                AddRobot(robotType, xOffset, 0, player, conn, new List<Instruction>()
                {
                    new Instruction_Move(MoveDirection.Up)
                });

                xOffset++;
            }
        }
    }

    private static void Test_Loop(NetworkConnection conn, PlayerController player)
    {
        InfectionManager.instance.AddBigInfectionAwayFromCities(wc.worldBuilder.GetCityOrReservedCoordinates());

        AddRobot(WorldController.instance.harvesterRobotPrefab, player.City.X + 1, player.City.Z + 3, player, conn, new List<Instruction>()
        {
            new Instruction_LoopStart(3),
            new Instruction_Move(MoveDirection.Down),
            new Instruction_LoopEnd(),
            new Instruction_Move(MoveDirection.Right)
        });
    }

    private static GameObject AddRobot(GameObject prefab, int x, int z, PlayerController owner, NetworkConnection conn, List<Instruction> instructions = null)
    {
        if (!WorldController.instance.worldBuilder.IsWithinWorld(x, z))
        {
            Debug.LogError("Tried to create scenario robot outside world boundary. Skipping");
            return null;
        }

        GameObject robotGO = WorldController.instance.SpawnObject(prefab, x, z, owner, conn);

        if (instructions != null)
        {
            RobotController robot = robotGO.GetComponent<RobotController>();
            robot.SetInstructions(instructions);
        }

        return robotGO;
    }

    private static void AddCopper(CityController city, int amount)
    {
        List<InventoryItem> startingResources = new List<InventoryItem>();
        for (int i = 0; i < amount; i++)
            startingResources.Add(new CopperItem());
        city.AddToInventory(startingResources, false);
    }

    private static void AddIron(CityController city, int amount)
    {
        List<InventoryItem> startingResources = new List<InventoryItem>();
        for (int i = 0; i < amount; i++)
            startingResources.Add(new IronItem());
        city.AddToInventory(startingResources, false);
    }

    private static void AddFood(CityController city, int amount)
    {
        List<InventoryItem> startingResources = new List<InventoryItem>();
        for (int i = 0; i < amount; i++)
            startingResources.Add(new FoodItem());
        city.AddToInventory(startingResources, false);
    }

}

public class ScenarioEntry
{
    public string FriendlyName;
    public Scenario ScenarioEnumRef;
    public Action<NetworkConnection, PlayerController> Run;

    public ScenarioEntry(string friendlyName, Scenario scenario, Action<NetworkConnection, PlayerController> run)
    {
        FriendlyName = friendlyName;
        ScenarioEnumRef = scenario;
        Run = run;
    }
}

// The scenarios has to be ordered from 0 and without number holes because they are also used as index in an GUI array
public enum Scenario
{
    Normal = 0,
    RichAndTeched = 1,
    WildPvE = 2,
    Test_AttackNeutralEnemy = 3,
    Test_LogisticsChain = 4,
    Test_StackingRobots = 5,
    Test_BatteryRobot = 6,
    Test_InfectionPurge = 7,
    Test_InfectionVictory = 8,
    Test_Crash = 9,
    Test_Loop = 10
}
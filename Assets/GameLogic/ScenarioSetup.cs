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

    static ScenarioSetup()
    {
        scenarios.Add(new ScenarioEntry("Normal", Scenario.Normal, Normal));
        scenarios.Add(new ScenarioEntry("Wild PvE", Scenario.WildPvE, WildPvE));
        scenarios.Add(new ScenarioEntry("Test Attack Neu Enemy", Scenario.Test_AttackNeutralEnemy, Test_AttackNeutralEnemy));
        scenarios.Add(new ScenarioEntry("Test Transporter", Scenario.Test_Harvester, Test_Harvester));
    }

    public static void RegisterWorldController(WorldController worldController)
    {
        wc = worldController;
    }

    public static void Run(int scenarioIndex, NetworkConnection conn, GameObject playerGO)
    {
        scenarios[scenarioIndex].Run(conn, playerGO);
    }

    public static void Run(Scenario scenario, NetworkConnection conn, GameObject playerGO)
    {
        ScenarioEntry scenarioEntry = scenarios.Where(s => s.ScenarioEnumRef == scenario).FirstOrDefault();
        if (scenarioEntry == null)
            throw new Exception("Not added support for this scenario enum entry.");
        else
            scenarioEntry.Run(conn, playerGO);
    }

    private static void Normal(NetworkConnection conn, GameObject playerGO)
    {
        PlayerCityController newPlayerCity = playerGO.GetComponent<PlayerCityController>();

        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingHarvesterRobots; i++)
            wc.SpawnHarvesterRobotWithClientAuthority(conn, newPlayerCity.X, newPlayerCity.Z, newPlayerCity);
        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingCombatRobots; i++)
            wc.SpawnCombatRobotWithClientAuthority(conn, newPlayerCity.X, newPlayerCity.Z, newPlayerCity);

        List<InventoryItem> startingResources = new List<InventoryItem>();
        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingCopper; i++)
            startingResources.Add(new CopperItem());
        for (int i = 0; i < Settings.Scenario_Normal_AmountOfStartingIron; i++)
            startingResources.Add(new IronItem());
        newPlayerCity.TransferToInventory(startingResources);
    }

    private static void WildPvE(NetworkConnection conn, GameObject playerGO)
    {
        Normal(conn, playerGO);

        GameObject neutralPlayerCityGO = wc.SpawnObject(wc.playerCityPrefab, 1, 1);
        PlayerCityController neutralPlayerCity = neutralPlayerCityGO.GetComponent<PlayerCityController>();

        for (int x = 0; x < wc.Width; x++)
        {
            GameObject combaRobotGO = wc.SpawnObject(wc.combatRobotPrefab, x, 0);
            CombatRobotController combatRobot = combaRobotGO.GetComponent<CombatRobotController>();
            neutralPlayerCity.AddOwnedGameObject(combaRobotGO);

            combatRobot.SetInstructions(new List<Instruction>
            {
                new Instruction_Move(MoveDirection.Random),
                new Instruction_Attack(AttackDirection.Random)
            });

            combatRobot.CmdStartRobot();
        }
    }

    private static void Test_AttackNeutralEnemy(NetworkConnection conn, GameObject playerGO)
    {
        PlayerCityController newPlayerCity = playerGO.GetComponent<PlayerCityController>();

        wc.SpawnObject(wc.combatRobotPrefab, newPlayerCity.X + 2, newPlayerCity.Z);

        GameObject combaRobotGO = wc.SpawnCombatRobotWithClientAuthority(conn, newPlayerCity.X, newPlayerCity.Z, newPlayerCity);
        CombatRobotController combatRobot = combaRobotGO.GetComponent<CombatRobotController>();
        combatRobot.SetInstructions(new List<Instruction>
        {
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Attack(AttackDirection.Right),
            new Instruction_Move(MoveDirection.Right),
            new Instruction_Attack(AttackDirection.Melee),
            new Instruction_Attack(AttackDirection.Melee),
            new Instruction_Attack(AttackDirection.Melee),
            new Instruction_Attack(AttackDirection.Melee)
        });
    }

    private static void Test_Harvester(NetworkConnection conn, GameObject playerGO)
    {
        PlayerCityController newPlayerCity = playerGO.GetComponent<PlayerCityController>();

        wc.SpawnResourceNode(wc.copperNodePrefab, newPlayerCity.X + 2, newPlayerCity.Z);

        GameObject harvesterGO = wc.SpawnHarvesterRobotWithClientAuthority(conn, newPlayerCity.X + 2, newPlayerCity.Z, newPlayerCity);
        HarvesterRobotController harvester = harvesterGO.GetComponent<HarvesterRobotController>();
        harvester.SetInstructions(new List<Instruction>
        {
            new Instruction_Harvest(),
            new Instruction_Move(MoveDirection.Left),
            new Instruction_DropInventory(),
            new Instruction_Move(MoveDirection.Right)
        });

        GameObject transporterGO = wc.SpawnTransporterRobotWithClientAuthority(conn, newPlayerCity.X + 1, newPlayerCity.Z, newPlayerCity);
        TransporterRobotController transporter = transporterGO.GetComponent<TransporterRobotController>();
        transporter.SetInstructions(new List<Instruction>
        {
            new Instruction_IdleUntilThen(UntilWhat.Full, new Instruction_Move(MoveDirection.Home)),
            new Instruction_DropInventory(),
            new Instruction_Move(MoveDirection.Right)
        });
    }

}

public class ScenarioEntry
{
    public string FriendlyName;
    public Scenario ScenarioEnumRef;
    public Action<NetworkConnection, GameObject> Run;

    public ScenarioEntry(string friendlyName, Scenario scenario, Action<NetworkConnection, GameObject> run)
    {
        FriendlyName = friendlyName;
        ScenarioEnumRef = scenario;
        Run = run;
    }
}

public enum Scenario
{
    Normal = 0,
    WildPvE = 1,

    Test_AttackNeutralEnemy = 100,
    Test_Harvester = 101
}
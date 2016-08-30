using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ScenarioSetup
{

    WorldController wc;

    public ScenarioSetup(WorldController worldController)
    {
        wc = worldController;
    }

    public void Normal(NetworkConnection conn, GameObject playerGO)
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

    public void AttackNeutralEnemy(NetworkConnection conn, GameObject playerGO)
    {
        PlayerCityController newPlayerCity = playerGO.GetComponent<PlayerCityController>();

        wc.SpawnObject(wc.combatRobotPrefab, newPlayerCity.X + 2, newPlayerCity.Z);

        GameObject combaRobotGO = wc.SpawnCombatRobotWithClientAuthority(conn, newPlayerCity.X, newPlayerCity.Z, newPlayerCity);
        CombatRobotController combatRobot = combaRobotGO.GetComponent<CombatRobotController>();
        combatRobot.SetInstructions(new List<string>
        {
            Instructions.MoveRight,
            Instructions.AttackRight,
            Instructions.MoveRight,
            Instructions.AttackMelee,
            Instructions.AttackMelee,
            Instructions.AttackMelee,
            Instructions.AttackMelee
        });
    }

    public void HarvesterTransporter(NetworkConnection conn, GameObject playerGO)
    {
        PlayerCityController newPlayerCity = playerGO.GetComponent<PlayerCityController>();

        wc.SpawnResourceNode(wc.copperNodePrefab, newPlayerCity.X + 2, newPlayerCity.Z);

        GameObject harvesterGO = wc.SpawnHarvesterRobotWithClientAuthority(conn, newPlayerCity.X + 2, newPlayerCity.Z, newPlayerCity);
        HarvesterRobotController harvester = harvesterGO.GetComponent<HarvesterRobotController>();
        harvester.SetInstructions(new List<string>
        {
            Instructions.Harvest,
            Instructions.MoveLeft,
            Instructions.DropInventory,
            Instructions.MoveRight,
        });

        GameObject transporterGO = wc.SpawnTransporterRobotWithClientAuthority(conn, newPlayerCity.X + 1, newPlayerCity.Z, newPlayerCity);
        TransporterRobotController transporter = transporterGO.GetComponent<TransporterRobotController>();
        transporter.SetInstructions(new List<string>
        {
            Instructions.IdleUntilDefined("FULL", "MOVE LEFT"),
            Instructions.DropInventory,
            Instructions.MoveRight
        });
    }

}

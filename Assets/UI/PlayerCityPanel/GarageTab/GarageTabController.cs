﻿using UnityEngine;
using System.Linq;

public class GarageTabController : MonoBehaviour
{
    public GameObject garageRobotPrefab;
    public GameObject leftColumn;
    public GameObject rightColumn;

    private CityController playerCityController;

    public void Show(CityController playerCityController)
    {
        this.playerCityController = playerCityController;
        this.playerCityController.Garage.ForEach(robot => AddRobotToGaragePanel(robot));
        this.playerCityController.OnRobotAddedToGarage += AddRobotToGaragePanel;
        this.playerCityController.OnRobotRemovedFromGarage += RemoveRobotFromGaragePanel;
    }

    public void Close()
    {
        playerCityController.Garage.ForEach(robot => RemoveRobotFromGaragePanel(robot));
        playerCityController.OnRobotAddedToGarage -= AddRobotToGaragePanel;
        playerCityController.OnRobotRemovedFromGarage -= RemoveRobotFromGaragePanel;
        playerCityController = null;
    }

    public void AddRobotToGaragePanel(RobotController robot)
    {
        GarageRobot existingGarageRobot = GetComponentsInChildren<GarageRobot>().Where(x => x.robot == robot).FirstOrDefault();
        if (existingGarageRobot)
            return;

        GameObject menuItemGO = Instantiate(garageRobotPrefab);
        menuItemGO.transform.SetParent(GetColumn(), false);

        GarageRobot garageRobot = menuItemGO.GetComponent<GarageRobot>();
        garageRobot.SetupGarageRobot(robot);
    }

    public void RemoveRobotFromGaragePanel(RobotController robot)
    {
        GarageRobot garageRobot = GetComponentsInChildren<GarageRobot>().Where(x => x.robot == robot).FirstOrDefault();

        if (!garageRobot)
            return;

        Destroy(garageRobot.gameObject);
        //Consider resetting columns for all menu items.
    }

    private Transform GetColumn()
    {
        if (leftColumn.transform.childCount > rightColumn.transform.childCount)
            return rightColumn.transform;
        else
            return leftColumn.transform;
    }
}

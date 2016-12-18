﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class StackingRobotsOverhangManager : MonoBehaviour
{
    public GameObject stackingRobotsOverhangPrefab;

    private PlayerCityController clientsOwnPlayerCity;

    private GameObject parent;
    private List<GameObject> spawnedGuiObjects = new List<GameObject>();

    public static StackingRobotsOverhangManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        parent = new GameObject("StackingRobotsOverhangs");
        parent.transform.SetParentToGO("ClientGameObjects");
    }

    public void DestroyAll()
    {
        spawnedGuiObjects.ForEach(go => Destroy(go));
    }

    public void Refresh()
    {
        if (!AttemptToFindPlayersOwnCity())
        {
            Debug.Log("Could not find player city, stacking robot GUI will not refresh");
            return;
        }

        DestroyAll();

        List<RobotController> currentStackCheck = new List<RobotController>();
        foreach (RobotController robot in FindObjectsOfType<RobotController>().Where(r => r.OwnerCity == clientsOwnPlayerCity && !r.IsStarted))
        {
            if (currentStackCheck.Count == 0 || robot.x == currentStackCheck[0].x && robot.z == currentStackCheck[0].z)
                currentStackCheck.Add(robot);
            else
            {
                if (currentStackCheck.Count > 1 || currentStackCheck[0].IsAtPlayerCity()) // Conditions that the current stack should show
                    SpawnOverhang(currentStackCheck);
                currentStackCheck.Clear();
            }
        }

        if (currentStackCheck.Count > 1 || (currentStackCheck.Count > 0 && currentStackCheck[0].IsAtPlayerCity())) // Conditions that the current stack should show
            SpawnOverhang(currentStackCheck);
    }

    private bool AttemptToFindPlayersOwnCity()
    {
        if (clientsOwnPlayerCity == null)
            clientsOwnPlayerCity = GameObjectUtils.FindClientsOwnPlayerCity();

        return clientsOwnPlayerCity != null;
    }

    private void SpawnOverhang(List<RobotController> robots)
    {
        GameObject stackingRobotsOverhangGO = Instantiate(stackingRobotsOverhangPrefab, parent.transform);
        StackingRobotsOverhangController stackingRobotsOverhangController = stackingRobotsOverhangGO.GetComponent<StackingRobotsOverhangController>();
        stackingRobotsOverhangController.Initiate(robots);

        spawnedGuiObjects.Add(stackingRobotsOverhangGO);
    }

}

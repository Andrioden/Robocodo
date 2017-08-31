using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class StackingRobotsOverhangOverview : MonoBehaviour
{
    public GameObject stackingRobotsOverhangPrefab;

    private GameObject parent;
    private List<GameObject> spawnedGuiObjects = new List<GameObject>();
    private int spawnedGuiObjectsHash;

    private float refreshInterval = 0.3f;

    public static StackingRobotsOverhangOverview instance;
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

        StartCoroutine(RefreshCoroutine());
    }

    public void DestroyAll()
    {
        spawnedGuiObjects.ForEach(go => Destroy(go));
        spawnedGuiObjects.Clear();
    }

    private IEnumerator RefreshCoroutine()
    {
        while (true)
        {
            Refresh();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    public void Refresh()
    {
        if (WorldController.instance.ClientsOwnPlayer() == null)
        {
            Debug.Log("Could not find player city, stacking robot GUI will not refresh. This debug message is OK if runs at the start. Might be data-sync timing issues.");
            return;
        }

        List<RobotController> ownedRobots = GameObject.FindGameObjectsWithTag("Robot")
            .Select(go => go.GetComponent<RobotController>())
            .Where(r => r.Owner == WorldController.instance.ClientsOwnPlayer() && (r.IsStill() || r.IsAtPlayerCity())).ToList();

        List<List<RobotController>> robotStacks = new List< List<RobotController> >();
        robotStacks.Add(new List<RobotController>());

        foreach (RobotController robot in ownedRobots.OrderBy(r => r.X).ThenBy(r => r.Z))
        {
            var lastRobotStack = robotStacks.Last();
            if (lastRobotStack.Count == 0 || robot.X == lastRobotStack[0].X && robot.Z == lastRobotStack[0].Z)
                lastRobotStack.Add(robot);
            else
            {
                robotStacks.Add(new List<RobotController>());
                robotStacks.Last().Add(robot);
            }
        }

        robotStacks = robotStacks.Where(s => s.Count > 1 || (s.Count == 1 && s[0].IsAtPlayerCity())).ToList(); // Filter away those with only 1 robot and not on city

        List<RobotController> robotStacksFlatList = robotStacks.SelectMany(s => s).ToList();
        if (IsRobotStackingChanged(robotStacksFlatList))
        {
            DestroyAll();

            foreach (var stack in robotStacks)
                SpawnOverhangPossibly(stack);

            spawnedGuiObjectsHash = GetHashFromRobots(robotStacksFlatList);
        }
    }

    private bool IsRobotStackingChanged(List<RobotController> currentRobots)
    {
        return spawnedGuiObjectsHash != GetHashFromRobots(currentRobots);
    }

    private int GetHashFromRobots(List<RobotController> robots)
    {
        var arrayOfRobotStrings = robots.OrderBy(r => r.netId.Value).Select(r => string.Format("{0}{1}{2}", r.netId.Value, r.X, r.Z)).ToArray();
        return string.Join(",", arrayOfRobotStrings).GetHashCode();
    }

    private void SpawnOverhangPossibly(List<RobotController> robots)
    {
        if (robots.Count < 1)
            return;

        GameObject stackingRobotsOverhangGO = Instantiate(stackingRobotsOverhangPrefab, robots[0].transform);
        StackingRobotsOverhangController stackingRobotsOverhangController = stackingRobotsOverhangGO.GetComponent<StackingRobotsOverhangController>();
        stackingRobotsOverhangController.Initialize(robots);

        spawnedGuiObjects.Add(stackingRobotsOverhangGO);
    }

}
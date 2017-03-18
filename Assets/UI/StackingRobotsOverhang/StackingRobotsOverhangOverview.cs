using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class StackingRobotsOverhangOverview : MonoBehaviour
{
    public GameObject stackingRobotsOverhangPrefab;

    private PlayerController _clientsOwnPlayer;

    private GameObject parent;
    private List<GameObject> spawnedGuiObjects = new List<GameObject>();

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

        WinLoseChecker.instance.OnLost += _ => DestroyAll();

        StartCoroutine(RefreshCoroutine());
    }

    public void DestroyAll()
    {
        spawnedGuiObjects.ForEach(go => Destroy(go));
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
        PlayerController clientPlayer = GetClientPlayer();
        if (clientPlayer == null)
        {
            Debug.Log("Could not find player city, stacking robot GUI will not refresh");
            return;
        }

        DestroyAll();

        List<RobotController> currentStackCheck = new List<RobotController>();

        List<RobotController> ownedRobots = GameObject.FindGameObjectsWithTag("Robot").Select(go => go.GetComponent<RobotController>()).Where(r => r.Owner == clientPlayer).ToList();
        foreach (RobotController robot in ownedRobots.OrderBy(r => r.x).ThenBy(r => r.z))
        {
            if (currentStackCheck.Count == 0 || robot.x == currentStackCheck[0].x && robot.z == currentStackCheck[0].z)
                currentStackCheck.Add(robot);
            else
            {
                SpawnOverhangPossibly(currentStackCheck);
                currentStackCheck.Clear();
                currentStackCheck.Add(robot);
            }
        }

        SpawnOverhangPossibly(currentStackCheck);
    }

    private PlayerController GetClientPlayer()
    {
        if (_clientsOwnPlayer == null)
            _clientsOwnPlayer = WorldController.instance.FindClientsOwnPlayer();

        return _clientsOwnPlayer;
    }

    private void SpawnOverhangPossibly(List<RobotController> robots)
    {
        if (robots.Count <= 1 || robots[0].IsAtPlayerCity())
            return;

        GameObject stackingRobotsOverhangGO = Instantiate(stackingRobotsOverhangPrefab, parent.transform);
        StackingRobotsOverhangController stackingRobotsOverhangController = stackingRobotsOverhangGO.GetComponent<StackingRobotsOverhangController>();
        stackingRobotsOverhangController.Initialize(robots);

        spawnedGuiObjects.Add(stackingRobotsOverhangGO);
    }

}
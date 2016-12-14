using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScavengerSpawner : MonoBehaviour
{

    public GameObject scavengerGroupPrefab;

    private IEnumerator spawnCoroutine;
    private WorldController wc;

    private List<ScavengerGroupController> scavengerGroupPool = new List<ScavengerGroupController>();

    public static ScavengerSpawner instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    private void Start()
    {
        wc = WorldController.instance;

        spawnCoroutine = Spawn();
        StartCoroutine(spawnCoroutine);
    }

    public IEnumerator Spawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(Settings.World_ScavengerSpawnInterval);

            if (scavengerGroupPool.Count(s => s.gameObject.activeSelf) < Settings.World_ScavengerMaxCount)
                ReusePoolOrSpawnNewGameObject();
        }
    }

    private void ReusePoolOrSpawnNewGameObject()
    {
        int posX = Utils.RandomInt(0, wc.Width);
        int posZ = Utils.RandomInt(0, wc.Height);

        ScavengerGroupController scavengerGroup = scavengerGroupPool.FirstOrDefault(s => !s.gameObject.activeSelf);

        if (scavengerGroup == null)
        {
            GameObject scavengerGroupGO = wc.SpawnObject(scavengerGroupPrefab, posX, posZ);
            scavengerGroup = scavengerGroupGO.GetComponent<ScavengerGroupController>();
            scavengerGroupPool.Add(scavengerGroup);
        }

        scavengerGroup.Initialize(posX, posZ);
    }

}
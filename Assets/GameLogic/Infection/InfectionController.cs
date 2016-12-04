using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

public class InfectionController : NetworkBehaviour
{

    private GameObject tileInfectionsParent;
    public GameObject tileInfectionPrefab;

    private int height;
    private int width;
    private SyncListTileInfection tileInfections = new SyncListTileInfection();

    private GameObject[,] tileInfectionGameObjects;

    public override void OnStartClient()
    {
        base.OnStartClient();

        tileInfectionsParent = new GameObject("TileInfections");
        tileInfections.Callback = SpawnOrUpdateTileInfection;
    }

    public void Initialize(int width, int height, List<Coordinate> cityOrReservedCoordinates)
    {
        this.width = width;
        this.height = height;
        tileInfectionGameObjects = new GameObject[width, height];

        SpawnBigInfectionAwayFromCities(cityOrReservedCoordinates);
    }

    private void SpawnBigInfectionAwayFromCities(List<Coordinate> cityOrReservedCoordinates)
    {
        int startSearchX = (width / 2) + Utils.RandomInt(width / -10, width / 10);
        int startSearchZ = (height / 2) + Utils.RandomInt(height / -10, height / 10);

        for (int x = startSearchX; x < width; x++)
        {
            for (int z = startSearchZ; z < height; z++)
            {
                if (!IsNearCityOrReserved(x, z, cityOrReservedCoordinates))
                {
                    SetOrUpdateInfection(x, z, 44);
                    return;
                }
            }
        }
    }

    private void SetOrUpdateInfection(int x, int z, int infection)
    {
        TileInfection? ti = tileInfections.Where(t => t.x == x && t.z == z).Cast<TileInfection?>().FirstOrDefault();
        if (ti.HasValue)
        {
            TileInfection tiValue = ti.Value;
            tiValue.infection = infection;
        }
        else
            tileInfections.Add(new TileInfection(x, z, infection));
    }

    private bool IsNearCityOrReserved(int x, int z, List<Coordinate> cityOrReservedCoordinates)
    {
        //int minDistanceFromCity = 0;
        return false;
    }

    private void SpawnOrUpdateTileInfection(SyncListTileInfection.Operation op, int index)
    {
        TileInfection ti = tileInfections[index];

        if (tileInfectionGameObjects[ti.x, ti.z] == null)
        {
            GameObject tileInfectionGO = (GameObject)Instantiate(tileInfectionPrefab, tileInfectionsParent.transform);
            tileInfectionGO.transform.position = new Vector3(ti.x, tileInfectionGO.transform.position.y, ti.z);
            tileInfectionGameObjects[ti.x, ti.z] = tileInfectionGO;
        }

        UpdateLol(tileInfectionGameObjects[ti.x, ti.z], ti);
    }

    private void UpdateLol(GameObject go, TileInfection ti)
    {
        Material goMaterial = go.GetComponent<Renderer>().material;
        goMaterial.color = new Color(goMaterial.color.r, goMaterial.color.g, goMaterial.color.b, ti.infection / 100.0f);
    }
}

public struct TileInfection
{
    public int x;
    public int z;
    public int infection; // 0 - 100, where 100 is max

    public TileInfection(int x, int z, int infection)
    {
        this.x = x;
        this.z = z;
        this.infection = infection;
    }
}

// TODO: Test to remove this class later
public class SyncListTileInfection : SyncListStruct<TileInfection>
{

}
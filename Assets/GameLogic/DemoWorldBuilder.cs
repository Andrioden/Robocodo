using UnityEngine;

public class DemoWorldBuilder : MonoBehaviour
{
    public GameObject worldControllerPrefab;

    public int width;
    public int height;
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;

    private void Awake()
    {
        /* This will temporarily destroy demo world but restore it when play mode is stopped. */
        DestroyDemoWorld();
    }

    public void SetupDemoWorld()
    {
        DestroyDemoWorld();

        if (transform.childCount > 0)
            return;

        NoiseConfig noiseConfig = new NoiseConfig() { Scale = scale, Octaves = octaves, Persistance = persistance, Lacunarity = lacunarity };

        GameObject worldControllerGameObject = (GameObject)Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        worldControllerGameObject.transform.parent = transform;
        worldControllerGameObject.GetComponent<WorldController>().BuildWorldDemoWorld(width, height, transform, noiseConfig);
    }

    public void DestroyDemoWorld()
    {
        transform.DestroyChildrenInEditor();
    }
}

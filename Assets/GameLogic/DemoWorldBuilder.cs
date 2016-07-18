using UnityEngine;
using Assets.GameLogic.ClassExtensions;

public class DemoWorldBuilder : MonoBehaviour
{
    public GameObject worldControllerPrefab;

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

        GameObject worldControllerGameObject = (GameObject)Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        worldControllerGameObject.transform.parent = transform;
        worldControllerGameObject.GetComponent<WorldController>().BuildWorldDemoWorld(30, 30, transform);
    }

    public void DestroyDemoWorld()
    {
        transform.DestroyChildrenInEditor();
    }
}

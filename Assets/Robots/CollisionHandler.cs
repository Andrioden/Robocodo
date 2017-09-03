using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public GameObject explotionPrefab;
    public GameObject rubblePrefab;

    private RobotController robotController;
    private GameObject rubble;

    void Start()
    {
        robotController = GetComponent<RobotController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        RobotController otherRobotController = other.transform.root.GetComponent<RobotController>();
        if (otherRobotController != null)
        {
            if (!otherRobotController.IsAtPlayerCity() && !robotController.IsAtPlayerCity())
            {
                Invoke("SpawnExplosion", 0.2f);
            }
        }
    }

    void SpawnExplosion()
    {
        Instantiate(explotionPrefab, transform.position + new Vector3(0, 0.4f, 0), Quaternion.identity);
        rubble = Instantiate(rubblePrefab, transform.position, transform.rotation);
        SetMaterialColor(robotController.Owner);
        robotController.meshGO.SetActive(false);
    }

    private void SetMaterialColor(PlayerController owner)
    {
        if (owner == null) //Neutral
            return;

        if (string.IsNullOrEmpty(owner.hexColor))
            return;

        var colorRenderers = rubble.GetComponentInChildren<ColorRenderers>();

        if (colorRenderers.renderers.Length == 0)
        {
            Debug.LogError("Rubble has no team color renderers. Won't be able to indicate team color. Set the rendere object to a GO that will be colored.");
            return;
        }

        var color = Utils.HexToColor(owner.hexColor);
        foreach (Renderer renderer in colorRenderers.renderers)
            renderer.material.color = color;
    }
}

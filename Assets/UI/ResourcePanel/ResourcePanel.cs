using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class ResourcePanel : MonoBehaviour
{

    public Text copperLabel;
    public Text ironLabel;

    private PlayerCityController localPlayerCity;

    public static ResourcePanel instance;
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
        InvokeRepeating("UpdateResourceLabels", 0, 0.3f); // Dont update it to often, so we use a slow updater
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void UpdateResourceLabels()
    {
        if (localPlayerCity != null)
        {
            copperLabel.text = "Copper: " + localPlayerCity.inventory.Count(i => i.GetType() == typeof(CopperItem));
            ironLabel.text = "Iron: " + localPlayerCity.inventory.Count(i => i.GetType() == typeof(IronItem));
        }
    }

    public void RegisterLocalPlayerCity(PlayerCityController playerCity)
    {
        localPlayerCity = playerCity;
    }
}
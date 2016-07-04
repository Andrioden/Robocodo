using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BuildableItem : MonoBehaviour
{
    public Type type;
    public int copperCost;
    public int ironCost;
    public Button button;

    void Start()
    {
        button.onClick.AddListener(BuyItem);
    }


    void BuyItem()
    {
        Debug.Log("BT");
        PlayerCityPanel.instance.GetCurrentlySelectedPlayerCity().BuyBuildableItem(type);
    }

    public enum Type
    {
        HARVESTER,
        ATTACKER
    }
}

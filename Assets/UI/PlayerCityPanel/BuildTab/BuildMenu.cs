using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildMenu : MonoBehaviour
{
    public GameObject buildableItemPrefab;
    public GameObject leftColumn;
    public GameObject rightColumn;

    public void AddBuildableItem(string name, UnityAction buyMethod, int copperCost, int ironCost, Sprite sprite)
    {
        GameObject menuItemGO = Instantiate(buildableItemPrefab);
        menuItemGO.transform.SetParent(GetColumn(), false);

        BuildableItem buildableItem = menuItemGO.GetComponent<BuildableItem>();
        buildableItem.SetupBuildableItem(name, buyMethod, copperCost, ironCost, sprite);
    }

    private Transform GetColumn()
    {
        if (leftColumn.transform.childCount > rightColumn.transform.childCount)
            return rightColumn.transform;
        else
            return leftColumn.transform;
    }
}

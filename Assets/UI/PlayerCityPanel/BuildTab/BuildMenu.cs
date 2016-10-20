using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;

public class BuildMenu : MonoBehaviour
{
    public GameObject buildableItemPrefab;
    public GameObject leftColumn;
    public GameObject rightColumn;

    Dictionary<string, Coroutine> _itemIDCoroutinePairs = new Dictionary<string, Coroutine>();
    private int _chainedPurchasesCount = 0;

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

    internal void IndicateSuccessfulPurchase(string robotName)
    {
        BuildableItem buildableItem = GetComponentsInChildren<BuildableItem>().Where(x => x.ID == robotName).FirstOrDefault();

        if (_itemIDCoroutinePairs.ContainsKey(buildableItem.ID))
            StopCoroutine(_itemIDCoroutinePairs[buildableItem.ID]);

        _itemIDCoroutinePairs[buildableItem.ID] = StartCoroutine(ChangeNameLabelOfBuildableItemTemporary(buildableItem, " BUILT"));
    }

    private IEnumerator ChangeNameLabelOfBuildableItemTemporary(BuildableItem buildableItem, string textToAppend)
    {
        _chainedPurchasesCount++;
        buildableItem.nameLabel.text = buildableItem.ID + textToAppend;

        if (_chainedPurchasesCount > 1)
            buildableItem.nameLabel.text += "(" + _chainedPurchasesCount + ")";

        yield return new WaitForSeconds(1f);
        buildableItem.nameLabel.text = buildableItem.ID;
        _chainedPurchasesCount = 0;

        _itemIDCoroutinePairs.Remove(buildableItem.ID);
    }
}

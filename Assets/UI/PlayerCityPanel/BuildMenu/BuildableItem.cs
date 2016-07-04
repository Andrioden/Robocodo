using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildableItem : MonoBehaviour
{
    public Button button;
    public Image icon;
    public Text copperCostLabel;
    public Text ironCostLabel;

    public void SetupBuildableItem(UnityAction buyAction, int copperCost, int ironCost, Sprite sprite)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buyAction);
        copperCostLabel.text = "X" + copperCost;
        ironCostLabel.text = "X" + ironCost;
        icon.sprite = sprite;
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuildableItem : MonoBehaviour
{
    public Button button;
    public Image icon;
    public Text nameLabel;
    public Text copperCostLabel;
    public Text ironCostLabel;

    public void SetupBuildableItem(string name, UnityAction buyAction, int copperCost, int ironCost, Sprite sprite)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buyAction);
        icon.sprite = sprite;
        copperCostLabel.text = "X" + copperCost;
        nameLabel.text = name.ToUpper();
        ironCostLabel.text = "X" + ironCost;
    }
}

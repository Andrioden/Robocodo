using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsPanel : MonoBehaviour
{
    public Button techTreeButton;

    private PlayerController localPlayer;

    public static ActionsPanel instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        techTreeButton.onClick.RemoveAllListeners();
        techTreeButton.onClick.AddListener(delegate { TechTreeDialog.instance.Show(localPlayer); });
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        localPlayer = player;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

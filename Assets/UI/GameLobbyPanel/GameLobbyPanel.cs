using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameLobbyPanel : NetworkBehaviour
{
    public GameObject panel;
    public GameObject playersColumn;
    public GameObject playerNickPanelPrefab;
    public Button startGameButton;

    public static GameLobbyPanel instance;

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

    private void Start()
    {
        //TODO: Refactor to find if we are host in some other way, maybe helper method in CustomNetworkManager.
        if (isServer)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGameButtonClicked);
        }
    }

    private void StartGameButtonClicked()
    {
        WorldTimeController.instance.StartGame();
    }

    public void Show()
    {
        panel.SetActive(true);
        InvokeRepeating("UpdatePlayerList", 0, 0.5f);
    }

    public void Hide()
    {
        CancelInvoke();
        panel.SetActive(false);
    }

    public void UpdatePlayerList()
    {
        if (!panel.activeSelf)
            return;

        playersColumn.transform.DestroyChildren();

        foreach (PlayerController playerController in WorldController.instance.FindPlayerControllers())
        {
            GameObject playerNickLabelGO = Instantiate(playerNickPanelPrefab);
            playerNickLabelGO.GetComponentInChildren<Text>().text = playerController.Nick;
            playerNickLabelGO.transform.SetParent(playersColumn.transform, false);
        }
    }
}
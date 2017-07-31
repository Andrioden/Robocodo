using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameLobbyPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject playersColumn;
    public GameObject playerNickPanelPrefab;
    public Button startGameButton;

    public static GameLobbyPanel instance;

    private DateTime lastUpdate;

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

    private void Update()
    {
        UpdatePlayerList();
    }

    public void Show(bool isServer)
    {
        panel.SetActive(true);

        if (isServer)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGameButtonClicked);
            startGameButton.gameObject.SetActive(true);
        }
        else
            startGameButton.gameObject.SetActive(false);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private void StartGameButtonClicked()
    {
        WorldTimeController.instance.StartGame();
    }

    private void UpdatePlayerList()
    {
        if (!panel.activeSelf)
            return;

        /* Could not use InvokeRepeating or Coroutine to update player list, because they both depend on Time moving forward. TimeScale is set to 0 while this panel is showing. */
        if (lastUpdate.AddMilliseconds(1000) > DateTime.Now)
            return;

        playersColumn.transform.DestroyChildren();

        foreach (PlayerController playerController in WorldController.instance.FindPlayerControllers())
        {
            if (string.IsNullOrEmpty(playerController.Nick))
                continue;

            GameObject playerNickLabelGO = Instantiate(playerNickPanelPrefab);
            playerNickLabelGO.transform.SetParent(playersColumn.transform, false);
            playerNickLabelGO.GetComponentInChildren<Text>().text = playerController.Nick;
        }

        lastUpdate = DateTime.Now;
    }
}
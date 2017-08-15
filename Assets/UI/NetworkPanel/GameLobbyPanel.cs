using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameLobbyPanel : MonoBehaviour
{
    public GameObject container;
    public GameObject playersColumn;
    public GameObject playerNickItemPrefab;
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

    private void Update()
    {
        if (LobbyManager.HasGameStarted && container.activeSelf)
            Hide();
    }

    public void Show(bool isServer)
    {
        container.SetActive(true);

        UpdatePlayerList(LobbyManager.Players);
        LobbyManager.OnPlayerListUpdated += UpdatePlayerList;

        if (isServer)
        {
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(LobbyManager.StartGame);
            startGameButton.gameObject.SetActive(true);
        }
        else
            startGameButton.gameObject.SetActive(false);
    }

    public void Hide()
    {
        LobbyManager.OnPlayerListUpdated -= UpdatePlayerList;
        container.SetActive(false);
    }

    private void UpdatePlayerList(List<LobbyPlayer> players)
    {
        playersColumn.transform.DestroyChildren();

        foreach (LobbyPlayer player in players)
        {
            GameObject playerNickLabelGO = Instantiate(playerNickItemPrefab);
            playerNickLabelGO.transform.SetParent(playersColumn.transform, false);
            playerNickLabelGO.GetComponentInChildren<Text>().text = player.Nick;
        }
    }

}
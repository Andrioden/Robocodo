using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;
using Assets.GameLogic.ClassExtensions;
using System;
using UnityEngine.Networking.Types;

public class NetworkPanel : MonoBehaviour
{
    private CustomNetworkManager manager;

    public Button quitButton;
    public InputField nickInput;
    public Button hostLanButton;
    public Button joinLanButton;
    public InputField MMGameNameField;
    public InputField MMGameSizeField;
    public Button hostMMutton;
    public Button findMMButton;
    public GameObject MMGameListContainer;
    public GameObject MMGameListJoinButtonPrefab;
    public Text feedbackText;

    public static NetworkPanel instance;
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
        manager = FindObjectOfType<CustomNetworkManager>();

        if (manager.matchMaker == null)
            manager.StartMatchMaker();

        if (nickInput.text.Length == 0)
            nickInput.text = "Andriod";

        if (MMGameSizeField.text.Length == 0)
            MMGameSizeField.text = "2";

        hostLanButton.onClick.RemoveAllListeners();
        hostLanButton.onClick.AddListener(LAN_OnHostClick);

        joinLanButton.onClick.RemoveAllListeners();
        joinLanButton.onClick.AddListener(LAN_OnJoinClick);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(OnQuitCLick);

        hostMMutton.onClick.RemoveAllListeners();
        hostMMutton.onClick.AddListener(MM_OnHostClick);

        findMMButton.onClick.RemoveAllListeners();
        findMMButton.onClick.AddListener(MM_OnFindGamesClick);

        ReadyToHostOrJoin();
    }

    private void LAN_OnHostClick()
    {
        feedbackText.text = "";
        if (RequireNick())
        {
            manager.StartHost();
            Debug.LogFormat("Hosting on {0}:{1}", manager.networkAddress, manager.networkPort);

            if (manager.isNetworkActive)
                Ingame();
            else
                feedbackText.text = "Hosting failed";
        }
    }

    private void LAN_OnJoinClick()
    {
        feedbackText.text = "";
        if (RequireNick())
        {
            manager.StartClient();
            Debug.LogFormat("Joining {0}:{1}", manager.networkAddress, manager.networkPort);
            feedbackText.text = "Joining...";
            Ingame();
        }
    }

    private void MM_OnHostClick()
    {
        feedbackText.text = "";
        if (RequireNick())
        {
            if (MMGameSizeField.text.Length == 0)
            {
                feedbackText.text = "Missing game player size input";
                return;
            }

            Debug.Log("Hosting matchmaking server " + MMGameNameField.text);
            uint matchSize = (uint)Convert.ToInt32(MMGameSizeField.text);
            manager.matchMaker.CreateMatch(MMGameNameField.text, matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
            Ingame();
        }
    }

    private void MM_OnFindGamesClick()
    {
        feedbackText.text = "";
        if (RequireNick())
        {
            manager.matchMaker.ListMatches(0, 20, "", true, 0, 0, MM_BuildGamesList);
        }
    }

    private void MM_BuildGamesList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        MMGameListContainer.transform.DestroyChildren();

        if (success)
        {
            Debug.LogFormat("Found {0} matches", matches.Count);
            foreach (var match in matches)
            {
                var matchGO = Instantiate(MMGameListJoinButtonPrefab) as GameObject;
                matchGO.transform.SetParent(MMGameListContainer.transform, false);
                Button matchGObutton = matchGO.GetComponent<Button>();
                matchGObutton.onClick.AddListener(() => { MM_OnJoinGameClick(match.networkId); });
                Text matchGObuttonText = matchGO.GetComponentInChildren<Text>();
                matchGObuttonText.text = match.name;
            }

            if (matches.Count == 0)
                feedbackText.text = "Found 0 matches";
            else
                feedbackText.text = "";
        }
        else
            feedbackText.text = "Failed to get match list";
    }

    private void MM_OnJoinGameClick(NetworkID networkId)
    {
        Debug.Log("Joining matchmaking game with match id: " + networkId);
        manager.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, manager.OnMatchJoined);
        feedbackText.text = "Joining...";
        Ingame();
    }

    private void OnQuitCLick()
    {
        feedbackText.text = "";

        if (NetworkServer.active || manager.IsClientConnected())
            manager.StopHost();
        else
            manager.StopClient();

        ReadyToHostOrJoin();
    }

    private bool RequireNick()
    {
        if (nickInput.text.Length > 0)
            return true;
        else
        {
            feedbackText.text = "No nick set!";
            return false;
        }
    }

    private void Ingame()
    {
        nickInput.gameObject.SetActive(false);
        hostLanButton.gameObject.SetActive(false);
        joinLanButton.gameObject.SetActive(false);
        MMGameNameField.gameObject.SetActive(false);
        MMGameSizeField.gameObject.SetActive(false);
        hostMMutton.gameObject.SetActive(false);
        findMMButton.gameObject.SetActive(false);
        MMGameListContainer.SetActive(false);
        quitButton.gameObject.SetActive(true);

        ResourcePanel.instance.Show();
    }

    private void ReadyToHostOrJoin()
    {
        nickInput.gameObject.SetActive(true);
        hostLanButton.gameObject.SetActive(true);
        joinLanButton.gameObject.SetActive(true);
        MMGameNameField.gameObject.SetActive(true);
        MMGameSizeField.gameObject.SetActive(true);
        hostMMutton.gameObject.SetActive(true);
        findMMButton.gameObject.SetActive(true);
        MMGameListContainer.SetActive(true);
        quitButton.gameObject.SetActive(false);

        ResourcePanel.instance.Hide();
    }

}

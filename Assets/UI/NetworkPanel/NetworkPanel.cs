using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;
using Assets.GameLogic.ClassExtensions;
using System;

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
        hostLanButton.onClick.AddListener(OnHostLanClick);

        joinLanButton.onClick.RemoveAllListeners();
        joinLanButton.onClick.AddListener(OnJoinLanClick);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(OnQuitCLick);

        hostMMutton.onClick.RemoveAllListeners();
        hostMMutton.onClick.AddListener(OnHostMatchMakingClick);

        findMMButton.onClick.RemoveAllListeners();
        findMMButton.onClick.AddListener(OnFindMatchMakingGameClick);

        ReadyToHostOrJoin();
    }

    private void OnHostLanClick()
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

    private void OnJoinLanClick()
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

    private void OnHostMatchMakingClick()
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
            CreateMatchRequest create = new CreateMatchRequest();
            create.name = MMGameNameField.text;
            create.size = (uint)Convert.ToInt32(MMGameSizeField.text);
            create.advertise = true;
            create.password = "";

            manager.matchMaker.CreateMatch(create, manager.OnMatchCreate);
            Ingame();
        }
    }

    private void OnFindMatchMakingGameClick()
    {
        feedbackText.text = "";
        if (RequireNick())
        {
            manager.matchMaker.ListMatches(0, 20, "", BuildMatchMakingGameList);
        }
    }

    private void BuildMatchMakingGameList(ListMatchResponse matchListResponse)
    {
        MMGameListContainer.transform.DestroyChildren();

        if (matchListResponse.success && matchListResponse.matches != null)
        {
            Debug.LogFormat("Found {0} matches", matchListResponse.matches.Count);
            foreach (var match in matchListResponse.matches)
            {
                var matchGO = Instantiate(MMGameListJoinButtonPrefab) as GameObject;
                matchGO.transform.SetParent(MMGameListContainer.transform, false);
                Button matchGObutton = matchGO.GetComponent<Button>();
                matchGObutton.onClick.AddListener(() => { OnJoinMatchMakingGameClick(match.networkId); });
                Text matchGObuttonText = matchGO.GetComponentInChildren<Text>();
                matchGObuttonText.text = match.name;
            }

            if (matchListResponse.matches.Count == 0)
                feedbackText.text = "Found 0 matches";
            else
                feedbackText.text = "";
        }
        else
            feedbackText.text = "Failed to get match list";
    }

    private void OnJoinMatchMakingGameClick(NetworkID id)
    {
        Debug.Log("Joining matchmaking game with match id: " + id);
        manager.matchMaker.JoinMatch(id, "", manager.OnMatchJoined);
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
    }

}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.Types;

public class NetworkPanel : MonoBehaviour
{
    private CustomNetworkManager networkManager;

    public Button quitButton;
    public InputField nickInput;
    public Dropdown gameModeDropdown;
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
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    private void Start()
    {
        networkManager = FindObjectOfType<CustomNetworkManager>();

        networkManager.SetMatchHost("eu1-mm.unet.unity3d.com", networkManager.matchPort, true); //TODO: Maybe make a user choice in the far future

        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();

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

        foreach (var scenario in ScenarioSetup.Scenarios)
            gameModeDropdown.options.Add(new Dropdown.OptionData(scenario.FriendlyName));

        ReadyToHostOrJoin();
    }

    private void LAN_OnHostClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput())
        {
            networkManager.StartHost();
            Debug.LogFormat("Hosting on {0}:{1}", networkManager.networkAddress, networkManager.networkPort);

            if (networkManager.isNetworkActive)
                Ingame();
            else
                feedbackText.text = "Hosting failed";
        }
    }

    private void LAN_OnJoinClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput())
        {
            networkManager.StartClient();
            Debug.LogFormat("Joining {0}:{1}", networkManager.networkAddress, networkManager.networkPort);
            feedbackText.text = "Joining...";
            Ingame();
        }
    }

    private void MM_OnHostClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput())
        {
            if (MMGameSizeField.text.Length == 0)
            {
                feedbackText.text = "Missing game player size input";
                return;
            }

            Debug.Log("Hosting matchmaking server " + MMGameNameField.text);
            uint matchSize = (uint)Convert.ToInt32(MMGameSizeField.text);
            networkManager.matchMaker.CreateMatch(MMGameNameField.text, matchSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
            Ingame();
        }
    }

    private void MM_OnFindGamesClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput())
        {
            Debug.LogFormat("Loading matches from match making server {0}:{1}", networkManager.matchHost, networkManager.matchPort);
            networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, MM_BuildGamesList);
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
        networkManager.matchMaker.JoinMatch(networkId, "", "", "", 0, 0, networkManager.OnMatchJoined);
        feedbackText.text = "Joining...";
        Ingame();
    }

    private void OnQuitCLick()
    {
        feedbackText.text = "";

        if (NetworkServer.active)
            networkManager.StopHost();
        else
            networkManager.StopClient();

        GameObject.Find("ClientGameObjects").transform.DestroyChildren();

        ReadyToHostOrJoin();
    }

    private bool ValidateMandatoryInput()
    {
        if (nickInput.text.Length <= 0)
        {
            feedbackText.text = "No nick set!";
            return false;
        }
        else
            return true;
    }

    private void Ingame()
    {
        nickInput.gameObject.SetActive(false);
        gameModeDropdown.gameObject.SetActive(false);
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
        gameModeDropdown.gameObject.SetActive(true);
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

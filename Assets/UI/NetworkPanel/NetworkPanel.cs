using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.Types;
using System.Linq;

public class NetworkPanel : MonoBehaviour
{
    private CustomNetworkManager networkManager;

    public GameObject mainMenuContainer;
    public Button leaveButton;
    public InputField nickInput;
    public Text aiCountLabel;
    public Slider aiCountSlider;
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
    public Button exitAppButton;

    public GameObject[] ingameUiGameObjects;

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

    // Is also run everytime the player leaves a game because leaving restarts the Scene, which reinitiates the scene Game Objects.
    private void Start()
    {
        Time.timeScale = 1;

        foreach (var scenario in ScenarioSetup.Scenarios)
            gameModeDropdown.options.Add(new Dropdown.OptionData(scenario.FriendlyName));

        networkManager = FindObjectOfType<CustomNetworkManager>();
        networkManager.SetMatchHost(PlayerSettings.MM_Server, networkManager.matchPort, true); //TODO: Maybe make a user choice in the far future
        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();

        nickInput.text = PlayerSettings.Game_Nick;
        gameModeDropdown.value = (int)PlayerSettings.Game_ScenarioChoice;
        MMGameSizeField.text = PlayerSettings.Game_Players.ToString();

        aiCountSlider.onValueChanged.RemoveAllListeners();
        aiCountSlider.onValueChanged.AddListener(OnAiCountSliderChange);
        OnAiCountSliderChange(aiCountSlider.value);

        hostLanButton.onClick.RemoveAllListeners();
        hostLanButton.onClick.AddListener(LAN_OnHostClick);

        joinLanButton.onClick.RemoveAllListeners();
        joinLanButton.onClick.AddListener(LAN_OnJoinClick);

        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnQuitNetworkGameCLick);

        hostMMutton.onClick.RemoveAllListeners();
        hostMMutton.onClick.AddListener(MM_OnHostClick);

        findMMButton.onClick.RemoveAllListeners();
        findMMButton.onClick.AddListener(MM_OnFindGamesClick);

        exitAppButton.onClick.RemoveAllListeners();
        exitAppButton.onClick.AddListener(OnExitAppClick);

        ActivateNetworkLobby();
    }

    public Scenario GetSelectedScenarioChoice()
    {
        return (Scenario)gameModeDropdown.value;
    }

    private void OnAiCountSliderChange(float newValue)
    {
        aiCountLabel.text = "AIs: " + (int)newValue;
    }

    private void LAN_OnHostClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput())
        {
            networkManager.StartHost();
            Debug.LogFormat("Hosting on {0}:{1}", networkManager.networkAddress, networkManager.networkPort);

            if (networkManager.isNetworkActive)
                ActivateIngame();
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
            ActivateIngame();
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
            ActivateIngame();
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

        ActivateIngame();
    }

    private void OnQuitNetworkGameCLick()
    {
        feedbackText.text = "";

        if (NetworkServer.active)
            networkManager.StopHost();
        else
            networkManager.StopClient();

        GameObject.Find("ClientGameObjects").transform.DestroyChildren();

        ActivateNetworkLobby();
    }

    private void OnExitAppClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
 Application.Quit();
#endif
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

    private void ActivateIngame()
    {
        SetMainMenuActive(false);

        SavePlayerSettings();
    }

    private void ActivateNetworkLobby()
    {
        SetMainMenuActive(true);
        SetIngameUIActive(false);
    }

    private void SetMainMenuActive(bool action)
    {
        mainMenuContainer.gameObject.SetActive(action);
        leaveButton.gameObject.SetActive(!action);
    }

    public void SetIngameUIActive(bool active)
    {
        ingameUiGameObjects.ToList().ForEach(go => go.SetActive(active));
    }

    private void SavePlayerSettings()
    {
        PlayerSettings.Game_Nick = nickInput.text;
        PlayerSettings.Game_ScenarioChoice = GetSelectedScenarioChoice();
        //PlayerSettings.Save(); // Not implemented
    }
}

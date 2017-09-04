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

    // MAIN MENU
    public GameObject mainMenuContainer;
    public Button[] leaveButtons;

    public InputField nickInput;
    public Dropdown gameModeDropdown;

    public Text maxPlayersLabel;
    public Slider maxPlayersSlider;
    public Text aiCountLabel;
    public Slider aiCountSlider;
    public InputField worldWidthField;
    public InputField worldHeightField;
    public InputField fpsLimitField;
    public Text fpsLimitHelpLabel;

    public Button hostLanButton;
    public Button joinLanButton;
    public InputField MMGameNameField;
    public Button hostMMutton;
    public Button findMMButton;
    public GameObject MMGameListContainer;
    public GameObject MMGameListJoinButtonPrefab;
    public Text feedbackText;
    public Button exitAppButton;

    // JOIN CONTAINER
    public GameObject joiningContainer;
    public Button abortJoinButton;

    // OTHER
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
        Debug.Log("Starting NetworkPanel");

        Time.timeScale = 1;

        gameModeDropdown.options.Clear();
        foreach (var scenario in ScenarioSetup.Scenarios)
            gameModeDropdown.options.Add(new Dropdown.OptionData(scenario.FriendlyName));

        networkManager = CustomNetworkManager.instance;
        networkManager.SetMatchHost(PlayerSettings.MM_Server, networkManager.matchPort, true); //TODO: Maybe make a user choice in the far future

        nickInput.text = PlayerSettings.Game_Nick;
        gameModeDropdown.value = (int)PlayerSettings.Game_ScenarioChoice;

        maxPlayersSlider.onValueChanged.RemoveAllListeners();
        maxPlayersSlider.onValueChanged.AddListener(OnMaxPlayersSliderChange);
        maxPlayersSlider.value = PlayerSettings.Game_Players;

        aiCountSlider.onValueChanged.RemoveAllListeners();
        aiCountSlider.onValueChanged.AddListener(OnAiCountSliderChange);
        aiCountSlider.value = PlayerSettings.Game_AIs;

        OnAiCountSliderChange(aiCountSlider.value);

        fpsLimitField.onValueChanged.AddListener(OnFpsLimitChange);
        fpsLimitField.onEndEdit.AddListener(EnsureValidFpsLimitValue);
        fpsLimitField.text = PlayerSettings.Graphics_MaxFPS.ToString();

        hostLanButton.onClick.RemoveAllListeners();
        hostLanButton.onClick.AddListener(LAN_OnHostClick);

        joinLanButton.onClick.RemoveAllListeners();
        joinLanButton.onClick.AddListener(LAN_OnJoinClick);

        foreach (Button leaveButton in leaveButtons)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(QuitNetworkGame);
        }

        hostMMutton.onClick.RemoveAllListeners();
        hostMMutton.onClick.AddListener(MM_OnHostClick);

        findMMButton.onClick.RemoveAllListeners();
        findMMButton.onClick.AddListener(MM_OnFindGamesClick);

        exitAppButton.onClick.RemoveAllListeners();
        exitAppButton.onClick.AddListener(OnExitAppClick);

        abortJoinButton.onClick.RemoveAllListeners();
        abortJoinButton.onClick.AddListener(OnAbortJoinClick);

        ActivateMainMenu();
    }

    public Scenario GetSelectedScenarioChoice()
    {
        return (Scenario)gameModeDropdown.value;
    }

    public void LobbyEventBehavior(LobbyManager.Event lobbyEvent)
    {
        if (lobbyEvent == LobbyManager.Event.ClientConnectionResponse_GameAlreadyStarted)
        {
            feedbackText.text = "";
            joiningContainer.SetActive(false);
        }
        else if (lobbyEvent == LobbyManager.Event.ClientConnectionResponse_OpenLobby)
        {
            GameLobbyPanel.instance.Show(LobbyManager.IsServer);
            feedbackText.text = "";
            joiningContainer.SetActive(false);
        }
        else if (lobbyEvent == LobbyManager.Event.ClientConnectionResponse_NoNick)
            AbortJoin("Cant join without a nick!");
        else if (lobbyEvent == LobbyManager.Event.ClientConnectionResponse_ServerFull)
            AbortJoin("Server is full");
        else if (lobbyEvent == LobbyManager.Event.ClientConnectionResponse_ServerNoFreePlayerPosition)
            AbortJoin("Server has no unused player positions");
    }

    public int MaxPlayers()
    {
        return (int)maxPlayersSlider.value;
    }

    private void OnMaxPlayersSliderChange(float newValue)
    {
        maxPlayersLabel.text = "Max players: " + (int)newValue;

        if (aiCountSlider.value > maxPlayersSlider.value - 1)
            aiCountSlider.value = maxPlayersSlider.value - 1;
    }

    private void OnAiCountSliderChange(float newValue)
    {
        aiCountLabel.text = "AIs: " + (int)newValue;

        if (maxPlayersSlider.value < aiCountSlider.value + 1)
            maxPlayersSlider.value = aiCountSlider.value + 1;
    }

    private void LAN_OnHostClick()
    {
        feedbackText.text = "";
        if (ValidateMandatoryInput() && ValidateHostInput())
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
            ActivateIngame();
            joiningContainer.SetActive(true);
        }
    }

    private void MM_OnHostClick()
    {
        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();

        feedbackText.text = "";
        if (ValidateMandatoryInput() && ValidateHostInput())
        {
            Debug.Log("Hosting matchmaking server " + MMGameNameField.text);
            uint matchSize = (uint)maxPlayersSlider.value;
            networkManager.matchMaker.CreateMatch(MMGameNameField.text, matchSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
            ActivateIngame();
        }
    }

    private void MM_OnFindGamesClick()
    {
        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();

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
        joiningContainer.SetActive(true);
    }

    public void QuitNetworkGame()
    {
        if (NetworkServer.active)
            networkManager.StopHost();
        else
            networkManager.StopClient();

        feedbackText.text = "";
        GameObject.Find("ClientGameObjects").transform.DestroyChildren();
        ActivateMainMenu();
    }

    private void OnExitAppClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
 Application.Quit();
#endif
    }

    private void OnAbortJoinClick()
    {
        AbortJoin("");
    }

    private void AbortJoin(string newFeedbackText)
    {
        networkManager.StopClient();

        Debug.Log("Aborting join!" + newFeedbackText);
        feedbackText.text = newFeedbackText;
        ActivateMainMenu();
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

    private bool ValidateHostInput()
    {
        string validationFailText = "";

        if (Convert.ToInt32(worldWidthField.text) < 1)
            validationFailText = "Width cant be lower than 1";
        else if (Convert.ToInt32(worldHeightField.text) < 1)
            validationFailText = "Height cant be lower than 1";

        if (validationFailText == "")
            return true;
        else
        {
            feedbackText.text = validationFailText;
            return false;
        }
    }

    private void ActivateIngame()
    {
        SetMainMenuActive(false);
        LimitFPS();
        SavePlayerSettings();
    }

    public void ActivateMainMenu()
    {
        MMGameListContainer.transform.DestroyChildren();
        SetMainMenuActive(true);
        SetIngameUIActive(false);
        joiningContainer.SetActive(false);
    }

    private void SetMainMenuActive(bool action)
    {
        mainMenuContainer.gameObject.SetActive(action);
        //foreach (Button leaveButton in leaveButtons)
        //    leaveButton.gameObject.SetActive(!action);
    }

    public void SetIngameUIActive(bool active)
    {
        ingameUiGameObjects.ToList().ForEach(go => go.SetActive(active));
    }

    private void LimitFPS()
    {
        //Setting max fps to keep the GPU from running at 100% when it's overkill. Might want to add this as an optional setting in the game menu when we create it.
        string maxFpsString = NetworkPanel.instance.fpsLimitField.text;
        Application.targetFrameRate = string.IsNullOrEmpty(maxFpsString) ? 0 : int.Parse(maxFpsString);
    }

    private void SavePlayerSettings()
    {
        PlayerSettings.Game_Nick = nickInput.text;
        PlayerSettings.Game_ScenarioChoice = GetSelectedScenarioChoice();
        PlayerSettings.Game_AIs = (int)aiCountSlider.value;
        PlayerSettings.Graphics_MaxFPS = string.IsNullOrEmpty(fpsLimitField.text) ? 0 : int.Parse(fpsLimitField.text);
        //PlayerSettings.Save(); // Not implemented
    }

    private void EnsureValidFpsLimitValue(string arg0)
    {
        if (string.IsNullOrEmpty(arg0))
            fpsLimitField.text = "0";

        int value;
        bool isInt = int.TryParse(fpsLimitField.text, out value);

        if (!isInt)
            fpsLimitField.text = "0";
    }

    private void OnFpsLimitChange(string arg0)
    {
        if (arg0 == "0")
            fpsLimitHelpLabel.enabled = true;
        else
            fpsLimitHelpLabel.enabled = false;
    }
}

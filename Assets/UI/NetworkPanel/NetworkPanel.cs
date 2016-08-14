using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkPanel : MonoBehaviour
{
    private CustomNetworkManager manager;

    public Text feedbackText;
    public InputField nickInput;
    public Button hostButton;
    public Button joinButton;
    public Button quitButton;

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

        if (nickInput.text.Length == 0)
            nickInput.text = "Andriod";
        hostButton.onClick.RemoveListener(OnHostClick);
        hostButton.onClick.AddListener(OnHostClick);
        joinButton.onClick.RemoveListener(OnJoinClick);
        joinButton.onClick.AddListener(OnJoinClick);
        quitButton.onClick.RemoveListener(OnQuitCLick);
        quitButton.onClick.AddListener(OnQuitCLick);

        ReadyToHostOrJoin();
    }

    private void OnHostClick()
    {
        feedbackText.text = "";

        if (nickInput.text.Length > 0)
        {
            manager.StartHost();
            Debug.LogFormat("Hosting on {0}:{1}", manager.networkAddress, manager.networkPort);

            if (manager.isNetworkActive)
                Ingame();
            else
                feedbackText.text = "Hosting failed";
        }
        else
            feedbackText.text = "No nick set!";
    }

    private void OnJoinClick()
    {
        feedbackText.text = "";

        if (nickInput.text.Length > 0)
        {
            manager.StartClient();
            Debug.LogFormat("Joining {0}:{1}", manager.networkAddress, manager.networkPort);
            feedbackText.text = "Joining...";
            Ingame();
        }
        else
            feedbackText.text = "No nick set!";
    }

    private void OnQuitCLick()
    {
        feedbackText.text = "";

        if (NetworkServer.active || manager.IsClientConnected())
            manager.StopHost();
        else
            manager.StopClient();

        ReadyToHostOrJoin();

        Destroy(GameObject.Find("Ground_NotNetwork"));
    }

    private void Ingame()
    {
        nickInput.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    private void ReadyToHostOrJoin()
    {
        nickInput.gameObject.SetActive(true);
        hostButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }

}

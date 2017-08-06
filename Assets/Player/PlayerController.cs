using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class PlayerController : NetworkBehaviour
{

    public GameObject cityRubblePrefab;

    [SyncVar]
    private string connectionID = ""; // It is string so we can set an AI friendly "fake" connection ID. I think, cant remember my reasoning fully.
    public string ConnectionID { get { return connectionID; } }

    private CityController _city;
    public CityController City
    {
        get
        {
            if (_city == null)
                _city = WorldController.instance.FindCityController(ConnectionID);
            return _city;
        }
    }

    [SyncVar]
    public string hexColor;

    [SyncVar]
    private string nick;
    public string Nick { get { return nick; } }

    [SyncVar]
    public double infectionContribution = 0;
    [SyncVar]
    public bool hasLost = false;

    private List<GameObject> ownedGameObjects = new List<GameObject>();
    public List<GameObject> OwnedGameObjects { get { return ownedGameObjects; } }

    private TechnologyTree techTree;
    public TechnologyTree TechTree { get { return techTree; } }


    // Use this for initialization
    private void Start()
    {
        if (isLocalPlayer)
        {
            ResourcePanel.instance.RegisterLocalPlayer(this);
            ActionsPanel.instance.RegisterLocalPlayer(this);
            MouseManager.instance.RegisterLocalPlayer(this);
            PositionCameraRelativeTo();
        }

        techTree = GetComponent<TechnologyTree>();
        techTree.Initialize(this);
    }

    [Server]
    public void Initialize(string connectionID, string nick, Color32 teamColor)
    {
        this.connectionID = connectionID.ToString();
        this.nick = nick;
        hexColor = Utils.ColorToHex(teamColor);
    }

    public void AddOwnedGameObject(GameObject go)
    {
        ownedGameObjects.Add(go);
    }

    public void ShowPopupForOwner(string text, Vector3 position, TextPopup.ColorType colorType)
    {
        if (isServer)
        {
            if (connectionToClient == null) // TODO: This is a temp solution to get AI feedback on player screen
            {
                if (Settings.Debug_EnableAiLogging) // Dont put on same if check as connectionToClient
                    TextPopupManager.instance.ShowPopupGeneric(ConnectionID + ": " + text, position, colorType.Color());
            }
            else
            {
                TargetShowPopup(connectionToClient, text, position, colorType.Color());
            }
        }
        else
            TextPopupManager.instance.ShowPopupGeneric(text, position, colorType.Color());
    }

    [TargetRpc]
    private void TargetShowPopup(NetworkConnection target, string text, Vector3 position, Color color)
    {
        TextPopupManager.instance.ShowPopupGeneric(text, position, color);
    }

    [Server]
    public void LostAndDestroy()
    {
        hasLost = true;
        Destroy(techTree);
        ownedGameObjects.ForEach(go => Destroy(go));

        WorldController.instance.SpawnObject(cityRubblePrefab, (int)transform.position.x, (int)transform.position.z);
    }

    private void PositionCameraRelativeTo()
    {
        RTSCamera.instance.PositionRelativeTo(transform);
    }

}
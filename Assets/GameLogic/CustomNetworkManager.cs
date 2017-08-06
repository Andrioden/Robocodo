using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;
using UnityEngine.Networking.NetworkSystem;

public class CustomNetworkManager : NetworkManager
{

    public GameObject worldControllerPrefab;

    public static CustomNetworkManager instance;
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

    public override void OnStartServer()
    {
        //Debug.Log("OnStartServer");
        LobbyManager.SetUpServerData();

        //base.OnStartServer(); // Maybe should call, dunno.
    }

    public override void OnStopServer()
    {
        //Debug.Log("OnStopServer");
        LobbyManager.ResetData();

        //base.OnStopServer(); // Maybe should call, dunno.
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        //Debug.Log("OnClientConnect on when isServer: " + LobbyManager.IsServer);
        LobbyManager.SetUpClientData(client);
        LobbyManager.OnClientEvent += NetworkPanel.instance.LobbyEventBehavior;

        ClientScene.Ready(conn);
        ClientScene.AddPlayer(conn, 0, new StringMessage(NetworkPanel.instance.nickInput.text)); // The ID input here is not used for anything
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //Debug.Log("OnClientDisconnect");
        LobbyManager.OnClientEvent -= NetworkPanel.instance.LobbyEventBehavior;
        LobbyManager.ResetData();

        NetworkPanel.instance.QuitNetworkGame();
    }

    public override void OnStopClient()
    {
        LobbyManager.OnClientEvent -= NetworkPanel.instance.LobbyEventBehavior;
        LobbyManager.ResetData();

        base.OnStopClient();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        LobbyManager.PlayerDisonnected(conn);

        // Will get disconnect when players StopsClient (https://forum.unity3d.com/threads/networkmanager-error-server-client-disconnect-error-1.439245/)
        base.OnServerDisconnect(conn);
    }

    /// <summary>
    /// playerControllerId is unique per player, multiple players can play from the same game instance, but since we have one player per connection we dont need to use it.
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        Debug.Log("Joined a player with connectionId: " + conn.connectionId.ToString());

        string nick = extraMessageReader.ReadMessage<StringMessage>().value;

        if (string.IsNullOrEmpty(nick))
        {
            LobbyManager.SendToClient(conn, LobbyManager.Event.ClientConnectionResponse_NoNick);
            return;
        }

        if (LobbyManager.HasGameStarted)
        {
            if (numPlayers >= NetworkPanel.instance.MaxPlayers())
                LobbyManager.SendToClient(conn, LobbyManager.Event.ClientConnectionResponse_ServerFull);
            else if (!WorldController.instance.worldBuilder.HasFreePlayerPosition())
                LobbyManager.SendToClient(conn, LobbyManager.Event.ClientConnectionResponse_ServerNoFreePlayerPosition);
            else
            {
                WorldController.instance.SpawnPlayer(conn, nick);
                LobbyManager.SendToClient(conn, LobbyManager.Event.ClientConnectionResponse_GameAlreadyStarted);
            }
        }
        else
        {
            LobbyManager.SendToClient(conn, LobbyManager.Event.ClientConnectionResponse_OpenLobby);
            LobbyManager.RegisterPlayer(conn, nick);
        }

        if (!Settings.Debug_EnableGameLobby && LobbyManager.IsServer && !LobbyManager.HasGameStarted)
            LobbyManager.StartGame();
    }

}
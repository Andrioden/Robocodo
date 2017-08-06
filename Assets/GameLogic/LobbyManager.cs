using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class LobbyManager
{

    private static bool isServer = false;
    public static bool IsServer { get { return isServer; } }

    private static NetworkClient client;

    private static bool hasGameStarted = false;
    public static bool HasGameStarted { get { return hasGameStarted; } }

    private static List<LobbyPlayer> players = new List<LobbyPlayer>();
    public static List<LobbyPlayer> Players { get { return players; } }

    public static event Action<Event> OnClientEvent = delegate { };
    public static event Action<LobbyEventMessage> OnClientMessage = delegate { };
    public static event Action<Event> OnServerEvent = delegate { };
    public static event Action<LobbyEventMessage> OnServerMessage = delegate { };
    public static event Action<List<LobbyPlayer>> OnPlayerListUpdated = delegate { };

    public static void SetUpServerData()
    {
        ResetData();

        NetworkServer.RegisterHandler(MsgType(), ServerReceive);
        isServer = true;
    }

    /// <summary>
    /// Server should also join and leave the lobby as clients do
    /// </summary>
    public static void SetUpClientData(NetworkClient setClient)
    {
        client = setClient;
        client.RegisterHandler(MsgType(), ClientReceive);
    }

    public static void StartGame()
    {
        int width = Convert.ToInt32(NetworkPanel.instance.worldWidthField.text);
        int height = Convert.ToInt32(NetworkPanel.instance.worldHeightField.text);

        GameObject worldControllerGameObject = GameObject.Instantiate(CustomNetworkManager.instance.worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        WorldController.instance.SetDimensions(width, height); // Set before network spawning world controller so width and height syncs for client correctly
        InfectionManager.instance.Initialize(width, height);

        NetworkServer.Spawn(worldControllerGameObject);

        NoiseConfig noiseConfig = new NoiseConfig() { Scale = 0.99f, Octaves = 4, Persistance = 0.3f, Lacunarity = 0.55f };
        WorldController.instance.BuildWorld(NetworkPanel.instance.MaxPlayers(), noiseConfig);

        WorldController.instance.GetComponent<ScavengerSpawner>().enabled = true;
        WorldController.instance.GetComponent<WinLoseChecker>().enabled = true;
        WorldController.instance.GetComponent<WorldTickController>().enabled = true;

        foreach(NetworkConnection conn in NetworkServer.connections)
        {
            LobbyPlayer lobbyPlayer = players.FirstOrDefault(p => p.ConnectionID == conn.connectionId);
            if (lobbyPlayer != null)
            {
                WorldController.instance.SpawnPlayer(conn, lobbyPlayer.Nick);
                SendToClient(conn, Event.ServerStartedGame);
            }
            else
                Debug.LogWarning("Could not find lobby player object for connectionID " + conn.connectionId);
        }

        for (int i = 0; i < (int)NetworkPanel.instance.aiCountSlider.value; i++)
            WorldController.instance.SpawnAI("AI " + (i + 1));

        hasGameStarted = true;
    }

    public static void PlayerDisonnected(NetworkConnection conn)
    {
        players.RemoveAll(p => p.ConnectionID == conn.connectionId);
        SyncPlayerListToClients();
    }

    public static void ResetData()
    {
        // Server stuff
        NetworkServer.UnregisterHandler(MsgType());
        isServer = false;

        // Client stuff
        if (client != null)
            client.UnregisterHandler(MsgType());
        client = null;

        // Common stuff
        hasGameStarted = false;
        players = new List<LobbyPlayer>();
    }

    /****************** BASE METHODS ******************/

    public static short MsgType()
    {
        return UnityEngine.Networking.MsgType.Highest + 1;
    }

    public static void SendToServer(LobbyEventMessage lobbyEventMessage)
    {
        //Debug.LogFormat("LobbyEventMessenger.SendToServer: {0}", lobbyEventMessage);

        client.Send(MsgType(), lobbyEventMessage);
    }

    public static void SendToAll(Event lobbyEvent)
    {
        SendToAll(new LobbyEventMessage { ev = lobbyEvent });
    }

    public static void SendToAll(LobbyEventMessage lobbyEventMessage)
    {
        NetworkServer.SendToAll(MsgType(), lobbyEventMessage);
    }

    public static void SendToClient(NetworkConnection conn, Event lobbyEvent)
    {
        SendToClient(conn, new LobbyEventMessage { ev = lobbyEvent });
    }

    public static void SendToClient(NetworkConnection conn, LobbyEventMessage lobbyEventMessage)
    {
        //Debug.LogFormat("LobbyEventMessenger.SendToClient {0}: {1}", conn.connectionId, lobbyEventMessage);
        NetworkServer.SendToClient(conn.connectionId, MsgType(), lobbyEventMessage);
    }

    private static void ServerReceive(NetworkMessage netMsg)
    {
        LobbyEventMessage lobbyEventMessage = netMsg.ReadMessage<LobbyEventMessage>();
        //Debug.LogFormat("LobbyEventMessenger.ServerReceive: {0}", lobbyEventMessage);

        OnServerEvent(lobbyEventMessage.ev);
        OnServerMessage(lobbyEventMessage);
    }

    /// <summary>
    /// Keep in mind Server is also an Client
    /// </summary>
    private static void ClientReceive(NetworkMessage netMsg)
    {
        LobbyEventMessage lobbyEventMessage = netMsg.ReadMessage<LobbyEventMessage>();
        //Debug.LogFormat("LobbyEventMessenger.ClientReceive: {0}", lobbyEventMessage);

        if (lobbyEventMessage.ev == Event.Sync_PlayerListToClients)
        {
            players = JsonConvert.DeserializeObject<List<LobbyPlayer>>(lobbyEventMessage.data);
            OnPlayerListUpdated(players);
        }
        else if (lobbyEventMessage.ev == Event.ServerStartedGame || lobbyEventMessage.ev == Event.ClientConnectionResponse_GameAlreadyStarted)
            hasGameStarted = true;

        OnClientEvent(lobbyEventMessage.ev);
        OnClientMessage(lobbyEventMessage);
    }

    /****************** CLIENT HELPERS ******************/

    


    /****************** SERVER HELPERS ******************/

    public static void RegisterPlayer(NetworkConnection conn, string nick)
    {
        players.Add(new LobbyPlayer(conn.connectionId, nick));
        SyncPlayerListToClients();
    }

    private static void SyncPlayerListToClients()
    {
        NetworkServer.SendToAll(MsgType(), new LobbyEventMessage() { ev = Event.Sync_PlayerListToClients, data = JsonConvert.SerializeObject(players) });
    }

    /****************** RELATED CLASSES ******************/

    public class LobbyEventMessage : MessageBase
    {
        public Event ev;
        public string data;

        public override string ToString()
        {
            return string.Format("[Event: {0}, Data: {1}]", ev, data);
        }
    }

    public enum Event
    {
        ClientConnectionResponse_GameAlreadyStarted,
        ClientConnectionResponse_OpenLobby,
        ClientConnectionResponse_NoNick,
        ClientConnectionResponse_ServerFull,
        ClientConnectionResponse_ServerNoFreePlayerPosition,

        Sync_PlayerListToClients,

        ServerStartedGame
    }
}

public class LobbyPlayer
{
    public int ConnectionID;
    public string Nick;

    public LobbyPlayer(int connectionID, string nick)
    {
        ConnectionID = connectionID;
        Nick = nick;
    }
}

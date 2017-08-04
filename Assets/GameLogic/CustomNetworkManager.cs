using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;


public class CustomNetworkManager : NetworkManager
{

    public GameObject worldControllerPrefab;

    public override void OnClientConnect(NetworkConnection conn)
    {
        client.RegisterHandler(MsgType.Highest + 5, ClientStatusMessenger.ClientReceive);
        ClientScene.Ready(conn);
        ClientScene.AddPlayer(0); // The ID input here is not used for anything
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        client.UnregisterHandler(MsgType.Highest + 5);
        NetworkPanel.instance.ActivateMainMenu();
    }

    /// <summary>
    /// No idea what is the point of playerControllerId, it is swallowed and not used. Instead the connectionId will always be unique.
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("Joined a player with connectionId: " + conn.connectionId.ToString());

        if (numPlayers == 0) // First player == HOSTING
        {
            int width = Convert.ToInt32(NetworkPanel.instance.worldWidthField.text);
            int height = Convert.ToInt32(NetworkPanel.instance.worldHeightField.text);

            GameObject worldControllerGameObject = Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            NetworkServer.Spawn(worldControllerGameObject);

            NoiseConfig noiseConfig = new NoiseConfig() { Scale = 0.99f, Octaves = 4, Persistance = 0.3f, Lacunarity = 0.55f };
            WorldController.instance.BuildWorld(width, height, MaxPlayers(), noiseConfig);

            InfectionManager.instance.Initialize(width, height);

            WorldController.instance.GetComponent<ScavengerSpawner>().enabled = true;
            WorldController.instance.GetComponent<WinLoseChecker>().enabled = true;
            WorldController.instance.GetComponent<WorldTickController>().enabled = true;

            for (int i = 0; i < (int)NetworkPanel.instance.aiCountSlider.value; i++)
                WorldController.instance.SpawnAI("AI " + (i + 1));
        }

        if (numPlayers >= MaxPlayers())
            ClientStatusMessenger.ServerSend(conn, ClientStatusMessenger.Status.Join_Disonnected_ServerFull);
        else if (!WorldController.instance.worldBuilder.HasFreePlayerPosition())
            ClientStatusMessenger.ServerSend(conn, ClientStatusMessenger.Status.Join_Disonnected_ServerNoFreePlayerPosition);
        else
        {
            WorldController.instance.SpawnPlayer(conn);
            ClientStatusMessenger.ServerSend(conn, ClientStatusMessenger.Status.Join_Connected);
        }
    }

    private int MaxPlayers()
    {
        return (int)NetworkPanel.instance.maxPlayersSlider.value;
    }

}

/// <summary>
/// Inspiration: https://forum.unity3d.com/threads/sending-messages-to-single-client.354616/
/// </summary>
public class ClientStatusMessenger
{

    public class ClientMessage : MessageBase
    {
        public Status message;
    }

    public enum Status
    {
        Join_Connected,
        Join_Disonnected_ServerFull,
        Join_Disonnected_ServerNoFreePlayerPosition
    }

    public static void ServerSend(NetworkConnection conn, Status clientMessageEnumValue)
    {
        ClientMessage msg = new ClientMessage() { message = clientMessageEnumValue };
        NetworkServer.SendToClient(conn.connectionId, MsgType.Highest + 5, msg);
    }

    public static void ClientReceive(NetworkMessage netMsg)
    {
        Status status = netMsg.ReadMessage<ClientMessage>().message;
        Debug.Log("OnClientMessageFromServer: " + status);
        if (status.ToString().Contains("Join_"))
            NetworkPanel.instance.JoinClientStatusMessage(status);
        else
            throw new Exception("Status not supported: " + status.ToString());
    }

}
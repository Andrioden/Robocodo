using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager : NetworkManager
{

    public GameObject worldControllerPrefab;

    public override void OnClientConnect(NetworkConnection conn)
    {
        ClientScene.Ready(conn);
        ClientScene.AddPlayer(0); // The ID input here is not used for anything
        NetworkPanel.instance.feedbackText.text = "";
    }

    /// <summary>
    /// No idea what happens with the playerControllerId, its swallowed and not used. Instead the connectionId will always be unique.
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("Joined a player with connectionId: " + conn.connectionId.ToString());

        if (numPlayers == 0) // First player == HOSTING
        {
            GameObject worldControllerGameObject = Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            NetworkServer.Spawn(worldControllerGameObject);

            WorldController.instance.BuildWorld(30, 40, 20);

            WorldController.instance.GetComponent<ScavengerSpawner>().enabled = true;
            WorldController.instance.GetComponent<WinLoseChecker>().enabled = true;

            WorldTickController.instance.StartGame();
        }

        WorldController.instance.SpawnPlayer(conn);
    }
}

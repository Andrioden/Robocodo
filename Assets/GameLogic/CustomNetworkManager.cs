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
        ClientScene.AddPlayer((short)numPlayers);
        NetworkPanel.instance.feedbackText.text = "";
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (numPlayers == 0)
        {
            GameObject worldControllerGameObject = (GameObject)Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            NetworkServer.Spawn(worldControllerGameObject);

            WorldController.instance.BuildWorld(30, 30, 10); //TODO: MatchSize; How can we get the correct number of players for the game?

            WorldTickController.instance.StartGame();
        }

        WorldController.instance.SpawnPlayer(conn, playerControllerId);
    }
}

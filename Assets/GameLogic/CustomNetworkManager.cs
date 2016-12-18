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
        if (numPlayers == 0) // Host
        {
            GameObject worldControllerGameObject = (GameObject)Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            NetworkServer.Spawn(worldControllerGameObject);

            WorldController.instance.BuildWorld(30, 40, 20);

            WorldController.instance.GetComponent<ScavengerSpawner>().enabled = true;
            WorldController.instance.GetComponent<WinLoseChecker>().enabled = true;

            WorldTickController.instance.StartGame();
        }

        WorldController.instance.SpawnPlayer(conn, playerControllerId);

        if (numPlayers > 0) // Client joined
            SyncGameStateToClient(conn);
    }

    private void SyncGameStateToClient(NetworkConnection conn)
    {
        foreach (RobotController robot in FindObjectsOfType<RobotController>())
            robot.TargetSetOwnerCity(conn, robot.GetOwnerCity().ownerConnectionId);
    }
}

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
            int width = 70;
            int height = 100;

            GameObject worldControllerGameObject = Instantiate(worldControllerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            NetworkServer.Spawn(worldControllerGameObject);

            NoiseConfig noiseConfig = new NoiseConfig() { Scale = 0.99f, Octaves = 4, Persistance = 0.3f, Lacunarity = 0.55f };
            WorldController.instance.BuildWorld(width, height, 20, noiseConfig);

            InfectionManager.instance.Initialize(width, height);

            WorldController.instance.GetComponent<ScavengerSpawner>().enabled = true;
            WorldController.instance.GetComponent<WinLoseChecker>().enabled = true;
            WorldController.instance.GetComponent<WorldTickController>().enabled = true;

            for (int i = 0; i < (int)NetworkPanel.instance.aiCountSlider.value; i++)
                WorldController.instance.SpawnAI("AI " + (i + 1));
        }

        WorldController.instance.SpawnPlayer(conn);
    }
}

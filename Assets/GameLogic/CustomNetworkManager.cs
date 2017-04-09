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
        ClientScene.Ready(conn);
        ClientScene.AddPlayer(0); // The ID input here is not used for anything
        NetworkPanel.instance.feedbackText.text = "";
        NetworkPanel.instance.joiningContainer.SetActive(false);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        NetworkPanel.instance.ActivateMainMenu();
    }

    /// <summary>
    /// No idea what happens with the playerControllerId, its swallowed and not used. Instead the connectionId will always be unique.
    /// </summary>
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        Debug.Log("Joined a player with connectionId: " + conn.connectionId.ToString());

        if (numPlayers == 0) // First player == HOSTING
        {
            int width = Convert.ToInt32(NetworkPanel.instance.worldWidthField.text);
            int height = Convert.ToInt32(NetworkPanel.instance.worldHeightField.text);
            int maxPlayers = (int)NetworkPanel.instance.maxPlayersSlider.value;

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
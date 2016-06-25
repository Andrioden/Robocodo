using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager : NetworkManager {

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnClientConnect(NetworkConnection conn)
    {
        ClientScene.Ready (conn);
        ClientScene.AddPlayer((short)numPlayers);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if(numPlayers == 0)
            WorldController.instance.BuildWorld();
        WorldController.instance.SpawnPlayerCity(conn, playerControllerId);
    }
}

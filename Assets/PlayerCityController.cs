using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerCityController : NetworkBehaviour, IClickable
{
    public MeshRenderer bodyMeshRenderer;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        bodyMeshRenderer.material.color = Color.blue;

        Camera.main.transform.parent.GetComponent<RTSCamera>().PositionRelativeToPlayer(transform);
    }

    public void Click()
    {
        if(hasAuthority)
            CmdSpawnHarvester((int)transform.position.x, (int)transform.position.z);    
    }

    [Command]
    public void CmdSpawnHarvester(int x, int z)
    {
        WorldController.instance.SpawnHarvesterWithClientAuthority(connectionToClient, x, z);
    }
}

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

        AdjustCamera();
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

    private void AdjustCamera()
    {
        RTSCamera camera = Camera.main.transform.parent.GetComponent<RTSCamera>();
        camera.PositionRelativeToPlayer(transform);
        camera.transform.localPosition += new Vector3(0, -15, 0);
    }

}

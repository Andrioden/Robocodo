using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerCityController : NetworkBehaviour, IClickable
{
    public MeshRenderer bodyMeshRenderer;

    public List<InventoryItem> inventory = new List<InventoryItem>();

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
            ResourcePanel.instance.RegisterLocalPlayerCity(this);

        if (isServer)
            AddToInventory(new List<InventoryItem>()
            {
                new CopperItem(),
                new CopperItem(),
                new CopperItem(),
                new IronItem(),
            });
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Server]
    public void AddToInventory(List<InventoryItem> items)
    {
        RpcAddToInventory(items.Select(i => i.Serialize()).ToArray());
    }
    [ClientRpc]
    private void RpcAddToInventory(string[] itemStrings)
    {
        foreach(string itemString in itemStrings)
        {
            if (itemString == CopperItem.SerializedType)
                inventory.Add(new CopperItem());
            else if (itemString == IronItem.SerializedType)
                inventory.Add(new IronItem());
            else
                throw new Exception("Forgot to add deserialization support for InventoryType: " + itemString);
        }
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

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class ResourceController : NetworkBehaviour, IClickable
{

    [SyncVar(hook = "OnRemainingItemsUpdated")]
    private int remainingItems;
    [SyncVar]
    public string resourceType;

    private Vector3 originalTransformScale;

    // Use this for initialization
    private void Start()
    {
        originalTransformScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        remainingItems = Utils.RandomInt(Settings.Resource_MinItemsPerNode, Settings.Resource_MaxItemsPerNode);
        UpdateTransformHeight();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Click()
    {
        Debug.LogFormat("This node of resouce type {0} has {1} remaining items", resourceType, remainingItems);
    }

    [Server]
    public void HarvestOneResourceItem()
    {
        remainingItems--;
        UpdateTransformHeight();
    }

    public int GetRemainingResourceItems()
    {
        return remainingItems;
    }

    private void OnRemainingItemsUpdated(int newRemainingItems)
    {
        remainingItems = newRemainingItems; // TODO FInd out why we have to set it, doesnt it update as part of SyncVar? Does hook stop that?
        UpdateTransformHeight();
    }

    private void UpdateTransformHeight()
    {
        int scalePercentage = MathUtils.LinearConversion(0, Settings.Resource_MaxItemsPerNode, 0, 100, remainingItems);
        float scaleFactor = scalePercentage / 100.0f;
        transform.localScale = new Vector3(originalTransformScale.x * scaleFactor, originalTransformScale.y * scaleFactor, originalTransformScale.z * scaleFactor);
    }

}

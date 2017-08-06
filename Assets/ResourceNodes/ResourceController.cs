using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public abstract class ResourceController : NetworkBehaviour, IClickable
{

    [SyncVar(hook = "OnRemainingItemsUpdated")]
    protected int remainingItems;
    public int RemainingItems { get { return remainingItems; } }

    public Transform physicalTransform;

    [SyncVar]
    private Vector3 originalTransformScale;

    // ********** SETTINGS : START **********

    public abstract string SerializedInventoryType();

    // ********** SETTINGS : END **********

    public override void OnStartServer()
    {
        originalTransformScale = new Vector3(physicalTransform.localScale.x, physicalTransform.localScale.y, physicalTransform.localScale.z);
    }

    // Use this for initialization
    protected virtual void Start()
    {
        remainingItems = Utils.RandomInt(Settings.World_Gen_ResourceItemsPerNode_Min, Settings.World_Gen_ResourceItemsPerNode_Max);
        UpdateTransformSize();
    }

    public void Click()
    {
        Debug.LogFormat("This node of resouce type {0} has {1} remaining items", GetType(), remainingItems);
    }

    public ClickablePriority ClickPriority()
    {
        return ClickablePriority.Low;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    [Server]
    public void HarvestOneResourceItem()
    {
        remainingItems--;
        UpdateTransformSize();
    }

    [Client]
    private void OnRemainingItemsUpdated(int newRemainingItems)
    {
        remainingItems = newRemainingItems;
        UpdateTransformSize();
    }

    [Client]
    protected void UpdateTransformSize()
    {
        double scalePercentage = MathUtils.LinearConversionDouble(0, Settings.World_Gen_ResourceItemsPerNode_Max, 0, 100, remainingItems);
        double volumeScaleFactor = scalePercentage / 100.0;
        float sideScaleFactor = (float)MathUtils.CubicRoot(volumeScaleFactor);

        physicalTransform.localScale = new Vector3(originalTransformScale.x * sideScaleFactor, originalTransformScale.y * sideScaleFactor, originalTransformScale.z * sideScaleFactor);
    }

    public override string ToString()
    {
        return string.Format("{0},{1}: {2} ", transform.position.x, transform.position.z, GetType());
    }

}
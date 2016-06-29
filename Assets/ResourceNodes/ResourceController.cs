using UnityEngine;
using System.Collections;
using System;

public class ResourceController : MonoBehaviour, IClickable
{

    public int remainingItems;
    public Type resourceType;

    // Use this for initialization
    private void Start()
    {
        remainingItems = Utils.RandomInt(Settings.Resource_MinItemsPerNode, Settings.Resource_MaxItemsPerNode);
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void Click()
    {
        Debug.LogFormat("This node of resouce type {0} has {1} remaining items", resourceType, remainingItems);
    }

}

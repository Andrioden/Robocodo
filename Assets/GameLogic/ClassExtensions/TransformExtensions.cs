using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;


public static class TransformExtensions
{
    public static Transform DestroyChildren(this Transform transform, bool networkObjects = false)
    {
        var tempList = transform.Cast<Transform>().ToList();

        foreach (Transform child in tempList)
        {
            if (networkObjects)
                NetworkServer.Destroy(child.gameObject);
            else
                GameObject.Destroy(child.gameObject);
        }

        return transform;
    }

    public static Transform DestroyChildrenInEditor(this Transform transform)
    {
        var tempList = transform.Cast<Transform>().ToList();

        foreach (Transform child in tempList)
            GameObject.DestroyImmediate(child.gameObject);

        return transform;
    }

    public static Transform SetParentToGO(this Transform transform, string name)
    {
        transform.parent = GameObject.Find("ClientGameObjects").transform;

        return transform;
    }
}
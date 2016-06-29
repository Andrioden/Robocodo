using UnityEngine;
using System.Collections;


public abstract class InventoryItem
{
    public abstract string Serialize();
}

public class CopperItem : InventoryItem
{
    public static readonly string SerializedType = "CopperItem";
    public override string Serialize()
    {
        return SerializedType;
    }
}

public class IronItem : InventoryItem
{
    public static readonly string SerializedType = "IronItem";
    public override string Serialize()
    {
        return SerializedType;
    }
}
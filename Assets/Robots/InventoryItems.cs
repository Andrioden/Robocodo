using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public abstract class InventoryItem
{
    // To force the implementator of this class to create the serialize pattern, later serializing might also include more info
    public abstract string Serialize();

    public static string[] Serialize(List<InventoryItem> inventory)
    {
        List<string> itemCounts = new List<string>();

        foreach (var itemCount in inventory.GroupBy(i => i.Serialize()))
            itemCounts.Add(itemCount.Key + "," + itemCount.Count());

        return itemCounts.ToArray();
    }

    public static InventoryItem DeserializeType(string serializedType)
    {
        if (serializedType == CopperItem.SerializedType)
            return new CopperItem();
        else if (serializedType == IronItem.SerializedType)
            return new IronItem();
        else if (serializedType == FoodItem.SerializedType)
            return new FoodItem();
        else
            throw new Exception("Forgot to add deserialization support for SerializedType string: " + serializedType);
    }

    public static List<InventoryItem> Deserialize(string[] serializedItemCounts)
    {
        List<InventoryItem> inventory = new List<InventoryItem>();

        foreach (string itemCount in serializedItemCounts)
        {
            string[] itemCountSplit = itemCount.Split(',');
            for (int i = 0; i < Convert.ToInt32(itemCountSplit[1]); i++)
                inventory.Add(DeserializeType(itemCountSplit[0]));
        }

        return inventory;
    }

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

public class FoodItem : InventoryItem
{
    public static readonly string SerializedType = "FoodItem";
    public override string Serialize()
    {
        return SerializedType;
    }
}
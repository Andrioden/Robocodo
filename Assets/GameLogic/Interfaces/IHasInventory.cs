using System.Collections;
using System.Collections.Generic;

public interface IHasInventory
{

    /// <summary>
    /// Should return the items not transfered to the inventory
    /// </summary>
    List<InventoryItem> AddToInventory(List<InventoryItem> items);

    /// <summary>
    /// Returns by the last in first out princple 
    /// </summary>
    List<InventoryItem> PickUp(int count);

}

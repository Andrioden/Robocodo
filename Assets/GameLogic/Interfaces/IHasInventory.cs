using System.Collections;
using System.Collections.Generic;

public interface IHasInventory
{

    bool HasOpenInventory();

    /// <summary>
    /// Should return the items not transfered to the inventory
    /// </summary>
    List<InventoryItem> AddToInventory(List<InventoryItem> items, bool playSoundEffect);

    /// <summary>
    /// Returns by the last in first out princple 
    /// </summary>
    List<InventoryItem> PickUp(int count);

}

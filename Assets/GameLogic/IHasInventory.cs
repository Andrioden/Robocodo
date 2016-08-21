using System.Collections;
using System.Collections.Generic;

public interface IHasInventory
{

    /// <summary>
    /// Should return the items not transfered to the inventory
    /// </summary>
    List<InventoryItem> TransferToInventory(List<InventoryItem> items);

}

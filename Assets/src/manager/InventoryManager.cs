using System.Linq;
using System.Threading;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventoryObject mainInventory;
    public InventoryObject hotbarInventory;
    public InventoryObject containerInventory;
    public InventoryObject equipmentInventory;

    public Transform container;
    public bool AddItemToPlayerInventory(ItemInstance item)
    {
        if (item.itemData == null) return false;
        int quantity = item.quantity;
        if (item.itemData.baseProperties.hotbarFirst)
        {
            quantity = hotbarInventory.AddItem(item);
            if (quantity > 0) quantity = mainInventory.AddItem(item);
        }
        else
        {
            quantity = mainInventory.AddItem(item);
            if (quantity > 0) quantity = hotbarInventory.AddItem(item);
        }

        if (quantity > 0) DropItem(item);
        return quantity == 0;
    }

    public void DropItem(ItemInstance item)
    {
        if (item.itemData == null) return;
        Vector3 dropPosition = GameObject.FindGameObjectWithTag("DropPoint").transform.position;
        GameObject droppedItem = Instantiate(item.itemData.baseProperties.prefab, dropPosition, Quaternion.identity);
        droppedItem.GetComponent<GroundItem>().item = item;
    }

    public bool TransferItem(UI_Slot fromSlot, InventoryObject toInventory)
    {
        if (fromSlot.slot == null || toInventory == null || fromSlot.slot.item.itemData == null) return false;
        ItemInstance itemToTransfer = fromSlot.slot.item;
        int remainingQuantity = itemToTransfer.quantity;

        // Try to stack the item in slots that already contain the same item
        foreach (var toSlot in toInventory.slots)
        {
            if (toSlot.uiSlot.output) continue;
            if (toSlot.item.itemData == itemToTransfer.itemData)
            {
                int availableSpace = toSlot.item.itemData.baseProperties.maxStackSize - toSlot.item.quantity;
                if (availableSpace > 0)
                {
                    int quantityToMove = Mathf.Min(remainingQuantity, availableSpace);
                    remainingQuantity -= quantityToMove;
                    MoveItem(fromSlot.slot, toSlot, quantityToMove);
                    if (remainingQuantity <= 0) return true; // All items have been transferred
                }
            }
        }

        // Try to place the item in an empty slot
        foreach (var toSlot in toInventory.slots)
        {
            if (toSlot.uiSlot.output) continue;
            if (toSlot.item.itemData == null)
            {
                if (toSlot.uiSlot.allowedItems.Length == 0 || toSlot.uiSlot.allowedItems.Contains(itemToTransfer.itemData))
                {
                    MoveItem(fromSlot.slot, toSlot, remainingQuantity);
                    return true; // Item has been transferred to an empty slot
                }
            }
        }

        Debug.Log("Unsuccessful transfer: No suitable slot found");
        return false; // Transfer was unsuccessful
    }


    private bool MoveItem(InventorySlot fromSlot, InventorySlot toSlot, int specifiedQuantity)
    {
        if (specifiedQuantity <= 0 || fromSlot.item.itemData == null) return false;
        int fromQuantity = fromSlot.item.quantity;
        int actualQuantityToMove = Mathf.Min(specifiedQuantity, fromQuantity, toSlot.item.itemData == null ? specifiedQuantity : toSlot.item.itemData.baseProperties.maxStackSize - toSlot.item.quantity);

        if (toSlot.item.itemData == null)
        {
            toSlot.item = new ItemInstance()
            {
                itemData = fromSlot.item.itemData,
                quantity = actualQuantityToMove,
            };
            toSlot.item.SetDurability(fromSlot.item.GetDurability());
        }
        else
        {
            toSlot.item.quantity += actualQuantityToMove;
        }

        fromSlot.item.quantity -= actualQuantityToMove;

        if (fromSlot.item.quantity <= 0) fromSlot.item.itemData = null;

        fromSlot.UpdateUI();
        toSlot.UpdateUI();

        return true;
    }

    public int RemoveItemAmount(ItemObject item, int quantityToRemove)
    {
        int remainingQuantity = quantityToRemove;

        remainingQuantity = mainInventory.RemoveItemAmount(item, remainingQuantity);
        if (remainingQuantity > 0)
        {
            remainingQuantity = hotbarInventory.RemoveItemAmount(item, remainingQuantity);
        }
        return remainingQuantity;
    }

    public bool RemoveItem(ItemInstance item, InventoryObject inventory)
    {
        return inventory.RemoveItem(item);
    }

    public int ItemQuantityInPlayerInventory(ItemObject item)
    {
        int totalQuantity = 0;
        totalQuantity += mainInventory.GetItemQuantity(item);
        totalQuantity += hotbarInventory.GetItemQuantity(item);
        return totalQuantity;
    }

    public bool ContainerOpen()
    {
        if (container == null) return false;
        return container.gameObject.activeSelf;
    }
}

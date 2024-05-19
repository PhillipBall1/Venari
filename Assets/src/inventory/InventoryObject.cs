using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "System/Items/New Inventory")]
public class InventoryObject : ScriptableObject
{
    public enum InventoryType
    {
        Hotbar,
        Player,
        Gear,
        Other,
        Split,
    }
    public InventoryType type;
    public List<InventorySlot> slots;
    public int slotCount;
    public List<ItemObject> acceptedItems;

    void OnEnable()
    {
        if (slots == null || slots.Count != slotCount)
        {
            slots = new List<InventorySlot>(new InventorySlot[slotCount]);

            for (int i = 0; i < slotCount; i++)
            {
                slots[i] = new InventorySlot(null);
            }
        }
    }


    public int AddItem(ItemInstance item)
    {
        int remainingQuantity = item.quantity;

        // First, try to stack the item with existing ones
        foreach (var slot in slots.Where(s => s.item.itemData == item.itemData))
        {
            if (slot.item.quantity < slot.item.itemData.baseProperties.maxStackSize)
            {
                int availableSpace = slot.item.itemData.baseProperties.maxStackSize - slot.item.quantity;
                int quantityToAdd = Mathf.Min(availableSpace, remainingQuantity);
                slot.item.quantity += quantityToAdd;
                remainingQuantity -= quantityToAdd;
                slot.UpdateUI();

                if (remainingQuantity <= 0) return 0;
            }
        }

        while (remainingQuantity > 0)
        {
            InventorySlot emptySlot = slots.FirstOrDefault(s => s.item.itemData == null);
            if (emptySlot != null)
            {
                int quantityToAdd = Mathf.Min(item.itemData.baseProperties.maxStackSize, remainingQuantity);
                emptySlot.item = new ItemInstance
                {
                    itemData = item.itemData,
                    quantity = quantityToAdd,
                    
                };
                emptySlot.item.SetDurability(item.GetDurability());
                if(item.itemData.baseProperties.type == ItemObject.ItemType.Ranged) emptySlot.item.SetCurrentAmmo(item.GetCurrentAmmo());
                remainingQuantity -= quantityToAdd;
                emptySlot.UpdateUI();
            }
            else
            {
                return remainingQuantity;
            }
        }

        return 0;
    }

    public int RemoveItemAmount(ItemObject item, int quantityToRemove)
    {
        
        foreach (var slot in slots)
        {
            if(slot.item.itemData != item) continue;

            if (slot.item.quantity > quantityToRemove)
            {
                slot.item.quantity -= quantityToRemove;
                slot.UpdateUI();
                return 0;
            }
            else
            {
                quantityToRemove -= slot.item.quantity;
                slot.item.itemData = null; 
                slot.UpdateUI();
                if (quantityToRemove == 0) break; 
            }
        }
        return quantityToRemove;
    }


    public bool RemoveItem(ItemInstance item)
    {
        int quantity = item.quantity;
        bool removed = false;

        foreach (var slot in slots)
        {
            if (slot.item.itemData != item.itemData) continue;

            if (slot.item.quantity > quantity)
            {
                slot.item.quantity -= quantity;
                removed = true;

                slot.UpdateUI();

                break;
            }
            else
            {
                quantity -= slot.item.quantity;
                slot.item.itemData = null;
                slot.item.quantity = 0;
                removed = true;

                slot.UpdateUI();

                if (quantity == 0) break;
            }
        }

        return removed;
    }

    public InventorySlot FindSlotWithItem(ItemObject item)
    {
        foreach (var slot in slots) 
        {
            if (slot.item.itemData == item) return slot;
        } 
        return null;
    }

    public bool HasSpaceForItem(ItemInstance item)
    {
        foreach (var slot in slots)
        {
            if (slot.uiSlot.allowedItems.Length > 0)
            {
                if (slot.uiSlot.allowedItems.Contains(item.itemData) && (slot.item.itemData == null || slot.item.quantity < item.itemData.baseProperties.maxStackSize))
                {
                    return true;
                }
            }
            else
            {
                if (slot.item.itemData == item.itemData && slot.item.quantity < item.itemData.baseProperties.maxStackSize) return true;
                if (slot.item.itemData == null) return true;
            }
        }
        return false;
    }

    public int GetItemQuantity(ItemObject item)
    {
        int totalQuantity = 0;

        foreach (var slot in slots)
        {
            if (slot.item.itemData != item) continue;

            totalQuantity += slot.item.quantity;
        }

        return totalQuantity;
    }

    public void Save(string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Create(path);
        formatter.Serialize(file, this);
        file.Close();
    }

    public void Load(string path)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            JsonUtility.FromJsonOverwrite((string)formatter.Deserialize(file), this);
            file.Close();
        }
    }
}

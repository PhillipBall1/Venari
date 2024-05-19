using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SlotClick : MonoBehaviour
{
    private UI_Slot uiSlot;

    private void Start()
    {
        uiSlot = GetComponent<UI_Slot>();
    }

    public void RightClick()
    {
        if (uiSlot.slot.item.itemData == null) return;
        InventoryObject targetInventory = DetermineTargetInventory(uiSlot.invObj.type, uiSlot.slot.item);
        if (targetInventory != null)
        {
            uiSlot.invManager.TransferItem(uiSlot, targetInventory);
        }
    }

    private InventoryObject DetermineTargetInventory(InventoryObject.InventoryType invType, ItemInstance currentItem)
    {
        InventoryObject containerInv = uiSlot.invManager.ContainerOpen() ? uiSlot.invManager.containerInventory : null;
        InventoryObject hotBarInv = uiSlot.invManager.hotbarInventory;
        InventoryObject playerInv = uiSlot.invManager.mainInventory;
        InventoryObject equipment = uiSlot.invManager.equipmentInventory;

        if (CanPlaceInEquipment(currentItem, equipment)) return equipment;

        switch (invType)
        {
            case InventoryObject.InventoryType.Player:

                if (containerInv != null && containerInv.HasSpaceForItem(currentItem)) return containerInv;
                else if (hotBarInv.HasSpaceForItem(currentItem) && currentItem.itemData.baseProperties.hotbarFirst) return hotBarInv;
                else if (hotBarInv.HasSpaceForItem(currentItem)) return hotBarInv;
                else if (playerInv.HasSpaceForItem(currentItem)) return playerInv;
                return null;
            case InventoryObject.InventoryType.Hotbar:
                if (containerInv != null && containerInv.HasSpaceForItem(currentItem)) return containerInv;
                else if (playerInv.HasSpaceForItem(currentItem)) return playerInv;
                else if (hotBarInv.HasSpaceForItem(currentItem)) return hotBarInv;
                return null;
            case InventoryObject.InventoryType.Other:
                if (hotBarInv.HasSpaceForItem(currentItem) && currentItem.itemData.baseProperties.hotbarFirst) return hotBarInv;
                else if (playerInv.HasSpaceForItem(currentItem)) return playerInv;
                else if (hotBarInv.HasSpaceForItem(currentItem)) return hotBarInv;
                return null;
            case InventoryObject.InventoryType.Gear:
                if (hotBarInv.HasSpaceForItem(currentItem) && currentItem.itemData.baseProperties.hotbarFirst) return hotBarInv;
                if (playerInv.HasSpaceForItem(currentItem)) return playerInv;
                if (hotBarInv.HasSpaceForItem(currentItem)) return hotBarInv;
                return null;
            default:
                return null;
        }
    }


    private bool CanPlaceInEquipment(ItemInstance currentItem, InventoryObject equipment)
    {
        if (currentItem.itemData.baseProperties.type != ItemObject.ItemType.Armor) return false;

        foreach (InventorySlot slot in equipment.slots)
        {
            if (slot.uiSlot.armorType != currentItem.itemData.armorProperties.armorType) continue;
            if (slot.item.itemData != null) return false;
            return equipment.HasSpaceForItem(currentItem);
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SlotDrag : MonoBehaviour
{
    private GameObject draggedItem;
    private UI_Slot uiSlot;

    private void Start()
    {
        uiSlot = GetComponent<UI_Slot>();
    }

    #region Dragging

    public string InitializeDrag(PointerEventData eventData, InventorySlot slot)
    {
        if (slot.item.itemData == null) return "Begin Drag: Slot item does not exist";
        
        if (uiSlot.invObj.type == InventoryObject.InventoryType.Gear) uiSlot.tempEquippedItem = slot.item;
        draggedItem = new GameObject("DraggedItem");
        Image image = draggedItem.AddComponent<Image>();
        image.sprite = slot.item.itemData.baseProperties.uiImage;
        image.raycastTarget = false;

        RectTransform rectTransform = draggedItem.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(75, 75);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        Canvas parent = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Canvas>();

        if (parent == null) return "Begin Drag: Parent Null";

        draggedItem.transform.SetParent(parent.transform, false);
        draggedItem.transform.SetAsLastSibling();
        SetDraggedPosition(eventData);
        return "";
    }

    public string Dragging(PointerEventData eventData)
    {
        if (draggedItem == null) return "On Drag: Dragged item does not exist";
        SetDraggedPosition(eventData);
        return "";
    }

    public string FinishDrag(PointerEventData eventData)
    {
        if (draggedItem != null) Destroy(draggedItem);
        else return "Drag End: Dragged item does not exist";

        
        // Check if the pointer raycast hit a valid GameObject
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            uiSlot.invManager.DropItem(uiSlot.slot.item);
            uiSlot.ClearSlot();
            UnequipItem();
            return "";
        }

        UI_Slot droppedOnSlotUI = eventData.pointerCurrentRaycast.gameObject.GetComponent<UI_Slot>();
        if (droppedOnSlotUI == null && droppedOnSlotUI == this) return "Drag End: Dropped on slot is null";

        InventoryObject toInv = droppedOnSlotUI.invObj == null ? droppedOnSlotUI.GetComponentInParent<UI_Inventory>().inventory: droppedOnSlotUI.invObj;
        InventoryObject fromInv = uiSlot.invObj;
        
        if (droppedOnSlotUI.invObj.type == InventoryObject.InventoryType.Split) return "";
        if (droppedOnSlotUI.output) return "Slot is an output";
        if (droppedOnSlotUI.slot.item == uiSlot.slot.item) return "";

        if (fromInv.type == InventoryObject.InventoryType.Split)
        {
            GameObject.FindGameObjectWithTag("ToolTip").GetComponent<UI_Tooltip>().SplitMade(uiSlot.slot.item.quantity);
        }

        if (toInv.type == InventoryObject.InventoryType.Gear)
        {
            if (droppedOnSlotUI.armorType != uiSlot.slot.item.itemData.armorProperties.armorType) return "Invalid armor type";

            uiSlot.playerEquipment.EquipGear(uiSlot.slot.item);
            uiSlot.SwapItems(droppedOnSlotUI.slot, true);
        }
        else if (fromInv.type == InventoryObject.InventoryType.Gear)
        {
            
            uiSlot.SwapItems(droppedOnSlotUI.slot, true);
            UnequipItem();
        } 
        else
        {
            if(droppedOnSlotUI.allowedItems != null && droppedOnSlotUI.allowedItems.Length > 0)
            {
                for (int i = 0; i < droppedOnSlotUI.allowedItems.Length; i++)
                {
                    if (uiSlot.slot.item.itemData == droppedOnSlotUI.allowedItems[i])
                    {
                        uiSlot.SwapItems(droppedOnSlotUI.slot);
                    } 
                }
            }
            else
            {
                uiSlot.SwapItems(droppedOnSlotUI.slot);
            }
        }
        return "";
    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        // Convert screen position of the mouse to world position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            draggedItem.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint);

        // Set the position of the dragged item
        draggedItem.transform.position = worldPoint;
    }

    private void UnequipItem()
    {
        if (uiSlot.tempEquippedItem != null)
        {
            uiSlot.playerEquipment.UnequipGear(uiSlot.tempEquippedItem);
            uiSlot.tempEquippedItem = null;
        }
    }

    #endregion

    #region Swapping

    public void LeftDrag(InventorySlot slot2)
    {
        // Check if the items are the same and can be stacked
        if (uiSlot.slot.item != null && slot2.item != null && uiSlot.slot.item.itemData == slot2.item.itemData && uiSlot.slot.item.itemData.baseProperties.maxStackSize > 1)
        {
            // Calculate the total quantity if stacked
            int totalQuantity = uiSlot.slot.item.quantity + slot2.item.quantity;

            if (totalQuantity <= uiSlot.slot.item.itemData.baseProperties.maxStackSize)
            {
                // If total quantity fits in one slot, stack them
                slot2.item.quantity = totalQuantity;
                uiSlot.slot.item = null; // This slot becomes empty
            }
            else
            {
                // If not, fill up the second slot to max and adjust the first slot
                uiSlot.slot.item.quantity = totalQuantity - uiSlot.slot.item.itemData.baseProperties.maxStackSize;
                slot2.item.quantity = uiSlot.slot.item.itemData.baseProperties.maxStackSize;
            }
        }
        else
        {
            // If items are different or cannot be stacked, perform a regular swap
            PerformSwap(slot2);
        }
        UpdateUIForBothSlots(uiSlot.slot, slot2);
    }


    public void RightDrag(InventorySlot slot2)
    {
        if (uiSlot.slot.item == null || uiSlot.slot.item.quantity <= 1) return;

        // Move only one item
        if (slot2.item == null)
        {
            // Clone item for slot2
            slot2.item = new ItemInstance()
            {
                itemData = uiSlot.slot.item.itemData,
                quantity = 1,
            };
            slot2.item.SetDurability(uiSlot.slot.item.GetDurability());
        }
        else if (slot2.item.itemData == uiSlot.slot.item.itemData && slot2.item.quantity < slot2.item.itemData.baseProperties.maxStackSize)
        {
            slot2.item.quantity++; // Safely add one to the stack
        }

        // Decrease quantity in the original slot
        uiSlot.slot.item.quantity--;

        // Clear the original slot if it becomes empty
        if (uiSlot.slot.item.quantity == 0)
        {
            uiSlot.slot.item = null;
        }
        UpdateUIForBothSlots(uiSlot.slot, slot2);
    }


    public void MiddleDrag(InventorySlot slot2)
    {
        if (uiSlot.slot.item == null || uiSlot.slot.item.quantity <= 1) return;

        int halfQuantity = uiSlot.slot.item.quantity / 2;
        int remainder = uiSlot.slot.item.quantity % 2;

        // Splitting into an empty slot
        if (slot2.item == null)
        {
            slot2.item = new ItemInstance()
            {
                itemData = uiSlot.slot.item.itemData,
                quantity = halfQuantity + remainder,
            };
            slot2.item.SetDurability(uiSlot.slot.item.GetDurability());
            uiSlot.slot.item.quantity = halfQuantity;
        }
        else if (slot2.item.itemData == uiSlot.slot.item.itemData) // Splitting into a non-empty slot
        {
            int combinedQuantity = slot2.item.quantity + halfQuantity + remainder;
            int maxStackSize = slot2.item.itemData.baseProperties.maxStackSize;

            if (combinedQuantity > maxStackSize)
            {
                int excess = combinedQuantity - maxStackSize;
                uiSlot.slot.item.quantity = halfQuantity + excess;
                slot2.item.quantity = maxStackSize;
            }
            else
            {
                slot2.item.quantity = combinedQuantity;
                uiSlot.slot.item.quantity = halfQuantity;
            }
        }
        else
        {
            // Handle the case where slot2 contains a different item
            Debug.LogError("Cannot split item: slot2 contains a different item");
            return;
        }

        Debug.Log("Current slot quantity: " + uiSlot.slot.item.quantity);
        Debug.Log("Transferred slot quantity: " + slot2.item.quantity);
        UpdateUIForBothSlots(uiSlot.slot, slot2);
    }

    public void PerformSwap(InventorySlot slot2)
    {
        ItemInstance tempItem = uiSlot.slot.item;
        uiSlot.slot.item = slot2.item;
        slot2.item = tempItem;
        UpdateUIForBothSlots(uiSlot.slot, slot2);
    }

    private void UpdateUIForBothSlots(InventorySlot slot1, InventorySlot slot2)
    {
        UI_Slot uiSlot1 = FindUISlotForInventorySlot(slot1);
        UI_Slot uiSlot2 = FindUISlotForInventorySlot(slot2);

        if (uiSlot1 != null) uiSlot1.UpdateSlotUI(slot1);
        if (uiSlot2 != null) uiSlot2.UpdateSlotUI(slot2);
    }

    public UI_Slot FindUISlotForInventorySlot(InventorySlot inventorySlot)
    {
        // Assuming you have a way to find all UI_Slot instances
        foreach (var uiSlot in FindObjectsOfType<UI_Slot>())
        {
            if (uiSlot.slot == inventorySlot)
            {
                return uiSlot;
            }
        }
        return null;
    }

    #endregion
}

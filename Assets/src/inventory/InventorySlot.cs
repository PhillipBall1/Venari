using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemInstance item;

    [NonSerialized]
    public UI_Slot uiSlot;
    
    public InventorySlot(ItemInstance item)
    {
        this.item = item;
    }

    public void SetUISlot(UI_Slot uiSlot)
    {
        this.uiSlot = uiSlot;
    }

    public void UpdateUI()
    {
        uiSlot?.UpdateSlotUI(this);
    }
}

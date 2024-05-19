using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    public InventoryObject inventory;
    public GameObject slotPrefab;
    public Transform slotsParent;

    [Header("Static Only")]
    public bool staticInventory = false;


    public bool container;


    private void Start()
    {
        if (container)
        {
            inventory = transform.parent.parent.GetComponent<Container>().inventory;
        }
        if (inventory != null && inventory.type == InventoryObject.InventoryType.Player)
        {
            InitializeInventoryUI();
        }
        if (staticInventory)
        {
            int count = 0;
            for(int i = 0; i < transform.childCount; i++)
            {
                
                UI_Slot slot = transform.GetChild(i).GetComponent<UI_Slot>();
                if (slot != null)
                {
                    slot.name = transform.name + i;
                    slot.SetInventorySlot(inventory.slots[i]);
                    inventory.slots[i].SetUISlot(slot);
                    if (inventory.type != InventoryObject.InventoryType.Gear) continue;
                    switch (count)
                    {
                        case 0: slot.armorType = ItemObject.ArmorType.Head; break;
                        case 1: slot.armorType = ItemObject.ArmorType.Chest; break;
                        case 2: slot.armorType = ItemObject.ArmorType.Legs; break;
                    }
                    count++;
                }
                else
                {
                    Debug.LogError("Failed to find UI_Slot component on instantiated slot prefab.");
                }
            }
        }
        
    }

    /*
    public void UpdateInventory(InventoryObject newInventory)
    {
        inventory = newInventory;

        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        InitializeInventoryUI();
    }
     */

    private void InitializeInventoryUI()
    {
        if (inventory == null) return;
        int count = 0;
        foreach (var inventorySlot in inventory.slots)
        {
            var uiSlotGameObject = Instantiate(slotPrefab, slotsParent);
            var uiSlot = uiSlotGameObject.GetComponent<UI_Slot>();
            uiSlotGameObject.name = transform.name + count;
            if (uiSlot != null)
            {
                uiSlot.SetInventorySlot(inventorySlot);
                inventorySlot.SetUISlot(uiSlot);
            }
            else
            {
                Debug.LogError("Failed to find UI_Slot component on instantiated slot prefab.");
            }
            count++;
        }
    }
}

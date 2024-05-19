using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHotbar : MonoBehaviour
{
    public List<UI_Slot> hotbarSlots;
    private PlayerEquip playerEquip;
    private PlayerHealth playerHealth;
    private PlayerStatus playerStatus;
    private PlayerBuilding playerBuild;
    private InventorySlot activeSlot;
    public int activeSlotNum = -1;
    private bool canEat = true;
    private void Start()
    {
        playerEquip = GetComponent<PlayerEquip>();
        playerHealth = GetComponent<PlayerHealth>();
        playerStatus = GetComponent<PlayerStatus>();
        playerBuild = GetComponent<PlayerBuilding>();
    }

    public void SlotOne() { SetActiveSlot(0); }

    public void SlotTwo() { SetActiveSlot(1); }

    public void SlotThree() { SetActiveSlot(2); }

    public void SlotFour() { SetActiveSlot(3); }

    public void SlotFive() { SetActiveSlot(4); }

    public void SlotSix() { SetActiveSlot(5); }

    public void SetActiveSlot(int slotPressed)
    {
        
        activeSlot = hotbarSlots[slotPressed].slot;
        activeSlotNum = slotPressed;
        if (activeSlot.item == null || activeSlot.item.itemData == null)
        {
            ClearSlots();
            return;
        }

        HandleItems(activeSlot.uiSlot);
        
    }

    private void HandleItems(UI_Slot slot)
    {
        foreach(UI_Slot slots in hotbarSlots)
        {
            slots.transform.GetChild(1).gameObject.SetActive(false);
        }
        slot.transform.GetChild(1).gameObject.SetActive(true);
        InventorySlot slotPressed = slot.slot;
        switch (slotPressed.item.itemData.baseProperties.type)
        {
            case ItemObject.ItemType.Food:
                playerEquip.ClearItemInHand();
                if (canEat)
                {
                    StartCoroutine(HandleFoodItem(slotPressed));
                    canEat = false;
                }
                break;
            case ItemObject.ItemType.Melee:
                playerEquip.SetActiveItem(slotPressed);
                break;
            case ItemObject.ItemType.Ranged:
                playerEquip.SetActiveItem(slotPressed);
                break;
            case ItemObject.ItemType.Medical:
                playerEquip.SetActiveItem(slotPressed);
                break;
            case ItemObject.ItemType.Construction:
                playerBuild.slot = slotPressed;
                playerBuild.SelectBuildingPart(slotPressed.item.itemData.name);

                break;
            default: playerEquip.ClearItemInHand(); break;
        }
    }

    private void OnEnable()
    {
        //UI_Slot.OnSlotUIUpdated += HandleSlotUIUpdated;
    }

    private void OnDisable()
    {
        //UI_Slot.OnSlotUIUpdated -= HandleSlotUIUpdated;
    }

    private void ClearSlots()
    {
        foreach (UI_Slot slots in hotbarSlots)
        {
            slots.transform.GetChild(1).gameObject.SetActive(false);
        }
        playerEquip.ClearItemInHand();
    }

    public InventorySlot GetActiveSlot() { return activeSlot; }


    private IEnumerator HandleFoodItem(InventorySlot slotPressed)
    {
        if (slotPressed.item.itemData.foodProperties.isCooked)
        {
            playerHealth.Heal(slotPressed.item.itemData.foodProperties.cookedInfo.cookedHealthRestore);
            playerStatus.Drink(slotPressed.item.itemData.foodProperties.cookedInfo.cookedThirstRestore);
            playerStatus.Eat(slotPressed.item.itemData.foodProperties.cookedInfo.cookedHungerRestore);
        }
        else
        {
            playerHealth.Heal(slotPressed.item.itemData.foodProperties.healthRestore);
            playerStatus.Drink(slotPressed.item.itemData.foodProperties.thirstRestore);
            playerStatus.Eat(slotPressed.item.itemData.foodProperties.hungerRestore);
        }
        if (!slotPressed.item.itemData.foodProperties.isLiquid)
        {
            AudioManager.Instance.PlayClipDataSound(slotPressed.item.itemData.baseProperties.typeAudio, "Consume", transform.position, 0);
        }
        else
        {
            AudioManager.Instance.PlayClipDataSound(slotPressed.item.itemData.baseProperties.typeAudio, "Consume", transform.position, 1);
        }
        slotPressed.item.quantity -= 1;
        slotPressed.UpdateUI();
        
        yield return new WaitForSeconds(1.3f);
        canEat = true;
    }
}

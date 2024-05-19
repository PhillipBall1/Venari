using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class UI_Tooltip : MonoBehaviour
{
    public GameObject tooltipPanel;
    public Image itemImage;
    public TMP_Text descriptionText;
    public TMP_Text nameText;
    public Transform propertiesParent;
    public GameObject propertyPrefab;
    public UI_Manager uiManager;

    public Transform splitSection;
    public TMP_Text splitText;
    public TMP_Text amountText;
    public Slider slider;
    public UI_Slot splitSlot;
    private UI_Slot currentSlot;

    void Awake()
    {
        HideTooltip();
    }

    private void Update()
    {
        if(uiManager.currentUIState == UI_Manager.UIState.None)
        {
            HideTooltip();
        }
        if(currentSlot != null && currentSlot.slot.item != null)
        {
            if (currentSlot.slot.item == null) HideTooltip();
        }
    }

    public void ShowTooltip(InventorySlot slot)
    {
        currentSlot = slot.uiSlot;
        
        ItemObject item = currentSlot.slot.item.itemData;
        if (item == null) return;

        bool isSingleItem = currentSlot.slot.item.quantity == 1;
        splitSlot.gameObject.SetActive(!isSingleItem);
        splitText.gameObject.SetActive(true);
        amountText.gameObject.SetActive(true);
        slider.gameObject.SetActive(true);

        if (isSingleItem)
        {
            if (item.baseProperties.hasDurability)
            {
                slider.interactable = false;
                slider.maxValue = item.durabilityProperties.durability.maxDurability;
                slider.value = item.durabilityProperties.durability.currentDurability;
                amountText.text = slider.value.ToString();
                splitText.text = "Durability";
            }
            else
            {
                slider.gameObject.SetActive(false);
                amountText.gameObject.SetActive(false);
                splitText.gameObject.SetActive(false);
            }
        }
        else
        {
            slider.interactable = true;
            slider.maxValue = currentSlot.slot.item.quantity;
            slider.value = currentSlot.slot.item.quantity / 2;
            amountText.text = slider.value.ToString();
            splitText.text = "Split Stack";

            splitSlot.slot.item = new ItemInstance
            {
                itemData = currentSlot.slot.item.itemData,
                quantity = (int)slider.value
            };
            splitSlot.UpdateSlotUI(splitSlot.slot);
        }

        itemImage.sprite = item.baseProperties.uiImage;
        descriptionText.text = item.baseProperties.description;
        nameText.text = item.name;

        foreach (Transform child in propertiesParent)
        {
            Destroy(child.gameObject);
        }

        switch (item.baseProperties.type)
        {
            case ItemObject.ItemType.Food: break;
            case ItemObject.ItemType.Melee: break;
            case ItemObject.ItemType.Ranged: break;
            case ItemObject.ItemType.Armor:
                switch (item.armorProperties.armorType)
                {
                    case ItemObject.ArmorType.Head: break;
                    case ItemObject.ArmorType.Chest: break;
                    case ItemObject.ArmorType.Legs: break;
                }
                break;
            case ItemObject.ItemType.Medical: break;
            case ItemObject.ItemType.Trap: break;
        }

        tooltipPanel.transform.localScale = Vector3.one;
    }

    private void CreateProperty(string name, float value, float maxValue, float minValue = 0)
    {
        GameObject property = Instantiate(propertyPrefab, propertiesParent);
        property.transform.GetChild(0).GetComponent<TMP_Text>().text = name;
        Slider slider = property.transform.GetChild(1).GetComponent<Slider>();
        property.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = value.ToString();
        slider.maxValue = maxValue;
        slider.minValue = minValue;
        slider.value = value;
    }

    public void SliderAmountChanged()
    {
        amountText.text = ((int)slider.value).ToString();
        if (splitSlot.slot.item == null) return;
        splitSlot.slot.item = new ItemInstance 
        { 
            itemData = currentSlot.slot.item.itemData,
            quantity= (int)slider.value
        };
        splitSlot.UpdateSlotUI(splitSlot.slot);
    }

    public void HideTooltip()
    {
        tooltipPanel.transform.localScale = Vector3.zero;
    }

    public void SplitMade(int amount)
    {
        currentSlot.slot.item.quantity= currentSlot.slot.item.quantity - amount;
        currentSlot.UpdateSlotUI(currentSlot.slot);
    }
}

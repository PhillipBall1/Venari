using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class UI_Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public InventorySlot slot;
    public Image itemIcon;
    public TMP_Text quantityText;
    public Slider slider;
    public Image broken;
    public ItemObject.ArmorType armorType;
    
    private MouseOperation mouseOp;

    private UI_SlotClick slotClicked;
    private UI_SlotDrag slotDragged;
    [NonSerialized]
    public UI_Manager uiManager;
    [NonSerialized]
    public InventoryManager invManager;
    public InventoryObject invObj;
    [NonSerialized]
    public PlayerHotbar playerHotBar;
    [NonSerialized]
    public PlayerEquipment playerEquipment;
    [NonSerialized]
    public ItemInstance tempEquippedItem;

    [Header("Container Only")]
    public ItemObject[] allowedItems;
    public bool output;
    public bool fuel;
    public static event Action<UI_Slot> OnSlotUIUpdated;
    [NonSerialized]
    public bool containerInv;

    public enum MouseOperation
    {
        None,
        LeftClick,
        RightClick,
        MiddleClick,
        LeftDrag,
        RightDrag,
        MiddleDrag
    }

    private void Awake()
    {
        uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_Manager>();
        invManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<InventoryManager>();
        playerHotBar = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHotbar>();
        playerEquipment = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerEquipment>();
        slotClicked = GetComponent<UI_SlotClick>();
        slotDragged = GetComponent<UI_SlotDrag>();
    }

    private void Start()
    {
        if (!containerInv) invObj = GetComponentInParent<UI_Inventory>().inventory;
    }

    private MouseOperation GetMouseOperation(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            return eventData.dragging ? MouseOperation.LeftDrag : MouseOperation.LeftClick;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            return eventData.dragging ? MouseOperation.RightDrag : MouseOperation.RightClick;
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            return eventData.dragging ? MouseOperation.MiddleDrag : MouseOperation.MiddleClick;
        }
        return MouseOperation.None;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (uiManager.GetDraggingSlot()) return;
        if (!HasItem()) return;
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "UI_Click", playerEquipment.transform.position);
        switch (GetMouseOperation(eventData))
        {
            case MouseOperation.LeftClick:
                uiManager.SelectedSlot(slot.uiSlot);
                break;
            case MouseOperation.RightClick:
                GetComponent<Animation>().Play("slotExit");
                slotClicked.RightClick();
                uiManager.SelectedSlot(null);
                break;
            case MouseOperation.MiddleClick:
                uiManager.SelectedSlot(null);
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (uiManager.GetDraggingSlot() || !HasItem()) return;
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "UI_Click", playerEquipment.transform.position);
        uiManager.SetDraggingSlot(true);
        DebugSlots(slotDragged.InitializeDrag(eventData, slot));
    }

    public void OnDrag(PointerEventData eventData)
    {
        DebugSlots(slotDragged.Dragging(eventData));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!HasItem()) return;
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "UI_Drop", playerEquipment.transform.position);
        uiManager.SetDraggingSlot(false);
        mouseOp = GetMouseOperation(eventData);
        DebugSlots(slotDragged.FinishDrag(eventData));
    }

    public void SwapItems(InventorySlot slot2, bool toEquip = false)
    {
        if (toEquip || slot.item.quantity == 1 || invObj.type == InventoryObject.InventoryType.Split)
        {
            slotDragged.LeftDrag(slot2);
        }
        else
        {
            switch (mouseOp)
            {
                case MouseOperation.LeftDrag:
                    slotDragged.LeftDrag(slot2);
                    break;
                case MouseOperation.RightDrag:
                    slotDragged.RightDrag(slot2);
                    break;
                case MouseOperation.MiddleDrag:
                    slotDragged.MiddleDrag(slot2);
                    break;
            }
        }
    }

    public void UpdateSlotUI(InventorySlot newSlotData)
    {
        
        slot = newSlotData; 

        if(!HasItem() || slot.item.quantity <= 0)
        {
            ClearSlot();
            return;
        }

        itemIcon.sprite = slot.item.itemData.baseProperties.uiImage;
        quantityText.text = slot.item.quantity > 1 ? slot.item.quantity.ToString() : "";
        itemIcon.enabled = true;
        quantityText.enabled = true;
        HandleSlider();
        HandleAmmo();

        if (playerHotBar != null && playerHotBar.GetActiveSlot() == slot)
        {
            //playerHotBar.SetActiveSlot(playerHotBar.activeSlotNum);
        }
        OnSlotUIUpdated?.Invoke(this);
    }

    public void UpdateInstance()
    {
        HandleSlider();
        HandleAmmo();
    }

    private void HandleAmmo()
    {
        if(slot.item.itemData.baseProperties.type == ItemObject.ItemType.Ranged)
        {
            quantityText.text = slot.item.GetCurrentAmmo().ToString();
        }
    }

    private void HandleSlider()
    {
        if (slot.item.itemData.baseProperties.hasDurability)
        {
            slider.transform.gameObject.SetActive(true);
            slider.maxValue = slot.item.GetDurability().maxDurability;
            slider.value = slot.item.GetDurability().currentDurability;
            if (slot.item.GetDurability().currentDurability == 0) broken.gameObject.SetActive(true);
            else broken.gameObject.SetActive(false);
        }
        else
        {
            slider.transform.gameObject.SetActive(false);
            broken.gameObject.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        slot.item = new ItemInstance();
        slot.item.itemData = null;
        itemIcon.sprite = null;
        quantityText.text = "";
        itemIcon.enabled = false;
        quantityText.enabled = false;
        slider.transform.gameObject.SetActive(false);
        broken.gameObject.SetActive(false);
        if (playerHotBar != null && playerHotBar.GetActiveSlot() == slot)
        {
            playerHotBar.SetActiveSlot(playerHotBar.activeSlotNum);
        }
    }

    public void SetInventorySlot(InventorySlot inventorySlot)
    {
        slot = inventorySlot;
        UpdateSlotUI(slot);
    }

    private void DebugSlots(string message)
    {
        if (message == "") return;
        Debug.LogError($"[UI_Slot]\n{message}\n" +
            $"Item Instance: {(slot.item != null ? slot.item : "None")}, Quantity: {(slot.item != null ? slot.item.quantity : "None")}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(slot.item.itemData == null) return;
        GetComponent<Animation>().Play("slotHover");
        AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.UI_Sounds, "PointerEnterSlot", playerEquipment.transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slot.item.itemData == null) return;
        GetComponent<Animation>().Play("slotExit");
    }

    public bool HasItem()
    {
        if(slot.item == null) return false;
        else if(slot.item != null && slot.item.itemData != null) return true;
        return false;
    }
}

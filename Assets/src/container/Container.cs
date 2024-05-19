using Gaia;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Container : Interactable
{
    [NonSerialized]
    public InventoryObject inventory;
    public ContainerType type;
    public Transform itemContainer;
    [Header("Cooker")]
    public float efficiency;
    public Button buttonPrefab;
    public Image cookSlider;
    public Transform VFX;
    public bool noEmissions;
    public AudioClip burnLoop;
    private List<UI_Slot> uiSlots;
    private UI_Manager uiManager;
    private InventoryManager invManager;
    private bool cooking = false;
    private Material emissionMat;
    private GameObject activeAudio;
    public enum ContainerType
    {
        Cooker,
        Box
    }

    private void Awake()
    {
        if (VFX != null) VFX.gameObject.SetActive(false);
        if(!noEmissions) emissionMat = GetComponent<MeshRenderer>().materials[0];
        GenerateInventoryObject();
        uiManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<UI_Manager>();
        invManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<InventoryManager>();
    }

    public override void OnInteract()
    {
        invManager.container = itemContainer;
        invManager.containerInventory = inventory;
        UI_CanvasMapping openContainer = transform.GetChild(0).GetComponent<UI_CanvasMapping>();
        uiManager.OpenContainer(openContainer);
        AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.UI_Sounds, "OpenCloseContainer", transform.position, 0);
        if (type == ContainerType.Cooker)
        {
            buttonPrefab.onClick.RemoveAllListeners();
            if (cooking) buttonPrefab.onClick.AddListener(() => StopCooking(buttonPrefab));
            if (!cooking) buttonPrefab.onClick.AddListener(() => StartCooking(buttonPrefab));
        }
    }

    #region COOKING

    private void StartCooking(Button startButton)
    {
        if (!cooking)
        {
            cooking = true;
            ChangeButton(false, startButton);
            StartCoroutine(CookItem(startButton));
        }
    }

    private IEnumerator CookItem(Button startButton)
    {
        InventorySlot woodSlot = inventory.FindSlotWithItem(AssetManager.Instance.database.GetItemByName("Wood"));
        UI_Slot itemSlot = uiSlots[1];
        UI_Slot coalSlot = uiSlots[2];
        UI_Slot productSlot = uiSlots[3];
        if (woodSlot == null) StopCooking(startButton);
        CookingActive(true);
        float woodCookTime = 2f;
        float itemCookTime = itemSlot.slot.item.itemData != null ? itemSlot.slot.item.itemData.cookableProperties.cookTime : 0f;
        float remainingWoodTime = woodCookTime;
        float remainingItemTime = itemCookTime * efficiency;
        bool reset = false;
        while (cooking && woodSlot.item != null)
        {
            if(woodSlot.item.itemData== null) StopCooking(startButton);
            if (!cooking) yield break;
            remainingWoodTime -= Time.deltaTime;

            if (itemSlot.slot.item.itemData != null)
            {
                if (!reset)
                {
                    itemCookTime = itemSlot.slot.item.itemData.cookableProperties.cookTime;
                    remainingItemTime = itemCookTime * efficiency;
                    reset = true;
                }
                remainingItemTime -= Time.deltaTime;
                cookSlider.fillAmount = 1f - (remainingItemTime / (itemCookTime * efficiency));
            }
            else
            {
                reset = false;
                cookSlider.fillAmount = 0f;
            }

            if (remainingWoodTime <= 0)
            {
                remainingWoodTime = woodCookTime;
                woodSlot.item.quantity--;
                if (coalSlot.slot.item.itemData == null)
                {
                    coalSlot.slot.item = new ItemInstance
                    {
                        itemData = AssetManager.Instance.database.GetItemByName("Coal"),
                        quantity = 1
                    };
                }
                else
                {
                    coalSlot.slot.item.quantity++;
                }
            }

            if (itemSlot.slot.item.itemData != null && itemSlot.slot.item.quantity > 0 && remainingItemTime <= 0)
            {
                remainingItemTime = itemCookTime * efficiency;

                int quantity = itemSlot.slot.item.itemData.cookableProperties.cookQuantity;
                int difference = itemSlot.slot.item.quantity - quantity;

                if (difference < 0) quantity = difference + quantity;

                if (productSlot.slot.item.itemData == null)
                {
                    productSlot.slot.item = new ItemInstance
                    {
                        itemData = itemSlot.slot.item.itemData.cookableProperties.product,
                        quantity = quantity
                    };
                    itemSlot.slot.item.quantity -= quantity;
                }
                else
                {
                    if (itemSlot.slot.item.itemData.cookableProperties.product != productSlot.slot.item.itemData)
                    {
                        reset = false;
                        cookSlider.fillAmount = 0f;
                        StopCooking(startButton);
                        yield return null;
                    }
                    else
                    {
                        productSlot.slot.item.quantity += quantity;
                        itemSlot.slot.item.quantity -= quantity;
                    }
                }
            }

            // Update UI
            woodSlot.UpdateUI();
            itemSlot.slot.UpdateUI();
            coalSlot.slot.UpdateUI();
            productSlot.slot.UpdateUI();

            yield return null;
        }
        StopCooking(startButton);
    }

    private void StopCooking(Button startButton)
    {
        if (cooking)
        {
            CookingActive(false);
            ChangeButton(true, startButton);
            cooking = false;
        }
    }

    private IEnumerator AdjustEmissionIntensity(bool increase)
    {
        float startIntensity = emissionMat.GetColor("_EmissiveColor").r;
        float targetIntensity = increase ? 500f : 0f;
        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (!cooking) // Check if the campfire has been turned off
            {
                targetIntensity = 0f; // Immediately set target intensity to 0
                break; // Break out of the loop to set the intensity to 0 immediately
            }
            elapsedTime += Time.deltaTime;
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
            emissionMat.SetColor("_EmissiveColor", Color.white * currentIntensity);
            yield return null;
        }

        emissionMat.SetColor("_EmissiveColor", Color.white * targetIntensity);
    }

    private void CookingActive(bool cooking)
    {
        if (VFX != null) VFX.gameObject.SetActive(cooking);
        if (!noEmissions) StartCoroutine(AdjustEmissionIntensity(cooking));

        if (cooking) activeAudio = AudioManager.Instance.PlayLoopingClipAtPosition(burnLoop, transform.position, AudioClipData.SoundLevel.Regular);
        if (!cooking && activeAudio != null) Destroy(activeAudio);

        cookSlider.fillAmount = 0;
    }

    private void ChangeButton(bool off, Button button)
    {
        button.image.color = off ? AssetManager.Instance.gColor : AssetManager.Instance.rColor;
        button.onClick.RemoveAllListeners();
        button.GetComponentInChildren<TMP_Text>().text = off ? "Start" : "Stop";
        if (off) button.onClick.AddListener(() => StartCooking(button));
        else button.onClick.AddListener(() => StopCooking(button));
    }


    #endregion

    private void GenerateInventoryObject()
    {
        uiSlots = new List<UI_Slot>();
        foreach (Transform child in itemContainer)
        {
            var uiSlot = child.GetComponent<UI_Slot>();
            if (uiSlot != null)
            {
                uiSlot.containerInv = true;
                uiSlots.Add(uiSlot);
            }
        }

        inventory = ScriptableObject.CreateInstance<InventoryObject>();
        inventory.type = InventoryObject.InventoryType.Other;
        inventory.name = name;
        inventory.slotCount = uiSlots.Count;
        inventory.slots = new List<InventorySlot>();

        for (int i = 0; i < uiSlots.Count; i++)
        {
            var newSlot = new InventorySlot(null);
            uiSlots[i].invObj = inventory;
            uiSlots[i].slot = newSlot;
            inventory.slots.Add(newSlot);
            uiSlots[i].name = itemContainer.name + i;
            uiSlots[i].SetInventorySlot(newSlot);
            newSlot.SetUISlot(uiSlots[i]);
            newSlot.UpdateUI();
        }
    }

}

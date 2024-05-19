using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Crafting : MonoBehaviour
{
    public ItemDatabase database;

    private List<ItemObject> displayedItems;

    public Transform craftingItemPrefab;
    public Transform displayParent;
    public Transform recipeContent;

    public Transform noActiveItem;
    public Transform activeItemTransform;
    public Transform recipeUIPrefab;
    public TMP_InputField quantityInput;
    public Button craftButton;
    public Transform queueItemPrefab;
    public Transform queueParent;
    private InventoryManager invManager;
    private ItemObject itemToCraft;
    private int quantityToCraft = 1;

    private void Start()
    {
        invManager = FindObjectOfType<InventoryManager>();
        displayedItems = new List<ItemObject>();
        SetItemsByType("All");
        noActiveItem.gameObject.SetActive(true);
        activeItemTransform.gameObject.SetActive(false);
    }

    public void SetItemsByType(string type)
    {
        displayedItems.Clear();
        foreach (ItemObject item in database.itemObjects)
        {
            if (!item.baseProperties.craftable) continue;

            if (type == "All") displayedItems.Add(item);
            else
            {
                if (item.baseProperties.type.ToString() != type) continue;

                displayedItems.Add(item);
            }
        }
        foreach(Transform child in displayParent)
        {
            Destroy(child.GetChild(0).gameObject);
            Destroy(child.gameObject);
        }
        foreach (ItemObject item in displayedItems)
        {
            Transform slot = Instantiate(craftingItemPrefab, displayParent);
            Button button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => SetActiveCraft(item));
            slot.GetChild(0).GetComponent<Image>().sprite = item.baseProperties.uiImage;
            slot.GetChild(1).GetComponent<TMP_Text>().text = item.name;
        }
    }

    public void SetActiveCraft(ItemObject item)
    {
        if(item == null)
        {
            noActiveItem.gameObject.SetActive(true);
            activeItemTransform.gameObject.SetActive(false);
            return;
        }
        noActiveItem.gameObject.SetActive(false);
        activeItemTransform.gameObject.SetActive(true);
        itemToCraft = item;
        activeItemTransform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = itemToCraft.baseProperties.uiImage;
        activeItemTransform.GetChild(1).GetComponent<TMP_Text>().text = itemToCraft.name;
        activeItemTransform.GetChild(2).GetComponent<TMP_Text>().text = itemToCraft.baseProperties.description;
        
        SetRecipes();
    }

    public void SetRecipes()
    {
        foreach (Transform child in recipeContent)
        {
            Destroy(child.GetChild(0).gameObject);
            Destroy(child.gameObject);
        }
        craftButton.interactable = true;
        foreach (RecipeItem recipeItem in itemToCraft.craftingProperties.recipeItems)
        {
            Transform recipeTransform = Instantiate(recipeUIPrefab, recipeContent);
            recipeTransform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = recipeItem.item.baseProperties.uiImage;
            recipeTransform.GetChild(2).GetComponent<TMP_Text>().text = recipeItem.item.name;
            recipeTransform.GetChild(3).GetComponent<TMP_Text>().text = (recipeItem.quantity * quantityToCraft).ToString();
            if (invManager.ItemQuantityInPlayerInventory(recipeItem.item) >= recipeItem.quantity * quantityToCraft)
            {
                recipeTransform.GetChild(2).GetComponent<TMP_Text>().color = AssetManager.Instance.gColor;
                craftButton.GetComponent<Image>().color = AssetManager.Instance.gColor;
            }
            else
            {
                recipeTransform.GetChild(2).GetComponent<TMP_Text>().color = AssetManager.Instance.rColor;
                craftButton.GetComponent<Image>().color = AssetManager.Instance.rColor;
                craftButton.interactable = false;
            }
        }
    }

    public void QuantityChanged()
    {
        if (quantityInput.text == "" || Int32.Parse(quantityInput.text) <= 0) quantityInput.text = "1";
        quantityToCraft = Int32.Parse(quantityInput.text);
        SetRecipes();
    }

    public void CraftItem()
    {
        if (queueParent.childCount >= 9) return;
        //Instantioate a queueItem
        Transform queueItem = Instantiate(queueItemPrefab, queueParent);
        queueItem.GetComponent<UI_QueueItem>().item = itemToCraft;
        queueItem.GetComponent<UI_QueueItem>().quantityToCraft = quantityToCraft;
        
    }

    public void IncreaseQuantityByX(int x)
    {
        int count = x + Int32.Parse(quantityInput.text);
        quantityInput.text = count.ToString();
    }
}

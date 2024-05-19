using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QueueItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemObject item;
    public int quantityToCraft;
    public TMP_Text timerText;
    private InventoryManager invManager;
    public bool crafting;
    private UI_Crafting uiCrafting; 

    private void Start()
    {
        transform.GetChild(0).GetComponent<Image>().sprite = item.baseProperties.uiImage;
        transform.GetChild(1).GetComponent<TMP_Text>().text = quantityToCraft.ToString();
        timerText.text = FormatTime(item.craftingProperties.craftingTime);
        invManager = FindObjectOfType<InventoryManager>();
        foreach (RecipeItem recipeItem in item.craftingProperties.recipeItems)
        {
            invManager.RemoveItemAmount(recipeItem.item, recipeItem.quantity * quantityToCraft);
        }
        uiCrafting = GameObject.FindGameObjectWithTag("Crafting").GetComponent<UI_Crafting>();
        uiCrafting.SetRecipes();
        crafting = false;
    }

    private void Update()
    {
        if(transform.parent.GetChild(0) == this.transform && !crafting)
        {
            StartCoroutine(QueueCraft(item.craftingProperties.craftingTime));
            crafting = true;
        }
    }

    private IEnumerator QueueCraft(int craftTime)
    {
        while (quantityToCraft > 0)
        {
            crafting = true;
            float remainingTime = craftTime;
            while (remainingTime > 0)
            {
                timerText.text = FormatTime((int)remainingTime);
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            quantityToCraft--;
            transform.GetChild(1).GetComponent<TMP_Text>().text = quantityToCraft.ToString();
            invManager.AddItemToPlayerInventory(new ItemInstance
            {
                itemData = item,
                quantity = item.craftingProperties.output
            });
        }
        crafting = false;
        Destroy(gameObject);
    }

    public void CancelCraft()
    {
        foreach (RecipeItem recipeItem in item.craftingProperties.recipeItems)
        {
            invManager.AddItemToPlayerInventory(new ItemInstance
            {
                itemData = recipeItem.item,
                quantity = recipeItem.quantity * quantityToCraft
            });
        }
        Destroy(gameObject);
        uiCrafting.SetRecipes();
    }

    private string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetChild(3).gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetChild(3).gameObject.SetActive(true);
    }
}

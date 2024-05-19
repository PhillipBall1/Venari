using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public List<UI_CanvasMapping> canvasMappings;
    public List<UI_Inventory> inventories;
    public UIState currentUIState;
    public UI_Tooltip tooltip;
    private bool draggingSlot;
    private InventoryManager invManager;

    public enum UIState
    {
        Menu,
        Inventory,
        Container,
        Crafting,
        None,
    }


    private void Start()
    {
        invManager = FindObjectOfType<InventoryManager>();
        SetUIState(UIState.None);
    }

    public void SetUIState(UIState newState)
    {
        currentUIState = newState;
        UpdateCanvasVisibility();
        UpdateCursorState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (currentUIState == UIState.Menu)
            SetUIState(UIState.None);
        else
            SetUIState(UIState.Menu);
    }

    public void ToggleInventory()
    {
        if (currentUIState == UIState.Inventory || currentUIState == UIState.Container)
            SetUIState(UIState.None);
        else
            SetUIState(UIState.Inventory);
    }

    public void ToggleCrafting()
    {
        if (currentUIState == UIState.Crafting)
            SetUIState(UIState.None);
        else
            SetUIState(UIState.Crafting);
    }

    public void OpenContainer(UI_CanvasMapping canvas)
    {
        canvasMappings[2] = canvas;
        SetUIState(UIState.Container);
        ShowInventoryWithContainer(); 
    }

    private void ShowInventoryWithContainer()
    {
        CanvasGroup group = canvasMappings[1].GetComponent<CanvasGroup>();
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void UpdateCanvasVisibility()
    {
        //playerHotbar.SetActiveSlot(playerHotbar.GetActiveSlot());
        for (int i = 0; i < canvasMappings.Count; i++)
        {
            if (canvasMappings[i] == null) continue;
            bool shouldBeActive = (int)currentUIState == i;
            CanvasGroup group = canvasMappings[i].GetComponent<CanvasGroup>();
            group.alpha = shouldBeActive ? 1 : 0;
            group.interactable = shouldBeActive;
            group.blocksRaycasts = shouldBeActive;

            if (i != 2) continue;

            if (shouldBeActive == true) continue;
            AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.UI_Sounds, "OpenCloseContainer", invManager.container.position, 1);
            canvasMappings[i] = null;
            invManager.container = null;
        }
    }

    private void UpdateCursorState()
    {
        if (currentUIState == UIState.None)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void CloseAllCanvas()
    {
        SetUIState(UIState.None);
    }

    public void SelectedSlot(UI_Slot selectedSlot)
    {
        //foreach (Transform slot in invManager.transform)
        //{
        //    slot.GetChild(1).gameObject.SetActive(false);
        //}
        //
        //if (selectedSlot == null) return;
        //if (selectedSlot.invObj.type != InventoryObject.InventoryType.Hotbar || selectedSlot.invObj.type != InventoryObject.InventoryType.Other)
        //{
        //    selectedSlot.transform.GetChild(1).gameObject.SetActive(true);
        //}
        if (selectedSlot == null || selectedSlot.slot == null || selectedSlot.slot.item == null || selectedSlot.slot.item.itemData == null)
        {
            tooltip.HideTooltip();
        }
        else
        {
            tooltip.ShowTooltip(selectedSlot.slot);
        }
    }

    public void SetDraggingSlot(bool drag){ draggingSlot = drag; }

    public bool GetDraggingSlot() { return draggingSlot; }
}

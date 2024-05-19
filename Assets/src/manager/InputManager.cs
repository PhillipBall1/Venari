using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public PlayerKeybinds keybinds;
    public Transform playerTransform;
    public Slider delayedActionSlider;
    private PlayerMovement playerMovement;
    private PlayerInteract playerInteract;
    private PlayerHotbar playerActiveItem;
    private PlayerAnimations playerAnimations;
    public UI_Manager UI;
    private PlayerBuilding playerBuilding;
    private UI_Crafting uiCrafting;
    private bool upgrading;
    private float elapsedTime;
    private void Start()
    {
        playerMovement = playerTransform.GetComponent<PlayerMovement>();
        playerInteract = playerTransform.GetComponent<PlayerInteract>();
        playerActiveItem = playerTransform.GetComponent<PlayerHotbar>();
        playerAnimations = playerTransform.GetComponent<PlayerAnimations>();
        playerBuilding = playerTransform.GetComponent<PlayerBuilding>();
        uiCrafting = FindObjectOfType<UI_Crafting>();
    }

    private void Update()
    {
        if (UI.currentUIState == UI_Manager.UIState.Menu) return;
        foreach (var keybindAction in keybinds.keybindActions)
        {
            if (Input.GetKeyDown(keybindAction.key))
            {
                StartAction(keybindAction.actionName);
            }
            if (Input.GetKeyUp(keybindAction.key))
            {
                StopAction(keybindAction.actionName);
            }
        }
        if (upgrading)
        {
            elapsedTime += Time.deltaTime;
            if(elapsedTime >= 4.5f)
            {
                delayedActionSlider.value = elapsedTime / 4.5f;
                playerBuilding.UpgradePart();
                upgrading = false;
            }
        }
        else
        {
            elapsedTime = 0;
        }
    }

    private void StartAction(string actionName)
    {
        switch (actionName)
        {
            case "Jump":
                playerMovement.Jump();
                playerAnimations.TriggerJumpStart();
                break;
            case "Sprint":
                playerMovement.StartSprinting();
                playerAnimations.StartSprinting();
                break;
            case "Crouch":
                playerMovement.StartCrouching();
                break;
            case "Interact":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerInteract.Interact();
                break;
            case "Slot One":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotOne();
                break;
            case "Slot Two":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotTwo();
                break;
            case "Slot Three":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotThree();
                break;
            case "Slot Four":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotFour();
                break;
            case "Slot Five":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotFive();
                break;
            case "Slot Six":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                playerActiveItem.SlotSix();
                break;
            case "Destroy Building":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                if (HoldingHammer()) BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                break;
            case "Upgrade Building":
                if (UI.currentUIState == UI_Manager.UIState.Crafting) return;
                if (HoldingHammer()) upgrading = true;
                break;
            case "Inventory":
                UI.ToggleInventory();
                break;
            case "Crafting":
                UI.ToggleCrafting();
                uiCrafting.SetActiveCraft(null);
                break;
        }
    }

    private void StopAction(string actionName)
    {
        switch (actionName)
        {
            case "Sprint":
                playerMovement.StopSprinting();
                playerAnimations.StopSprinting();
                break;
            case "Crouch":
                playerMovement.StopCrouching();
                break;
            case "Upgrade Building":
                upgrading = false;
                break;
        }
    }

    private bool HoldingHammer()
    {
        if (playerActiveItem.GetActiveSlot().item == null || playerActiveItem.GetActiveSlot().item.itemData == null) return false;
        if (playerActiveItem.GetActiveSlot().item.itemData.name == "Hammer") return true;
        return false;
    }
}

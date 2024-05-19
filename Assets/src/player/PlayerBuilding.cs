using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Placer;

public class PlayerBuilding : MonoBehaviour
{
    public InventorySlot slot;
    private PlayerHotbar playerHotbar;

    private void Start()
    {
        playerHotbar = GetComponent<PlayerHotbar>();
    }

    private void Update()
    {
        if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.PLACE)
        {
            Place();
        }
        else if (BuildingPlacer.Instance.GetBuildMode == BuildingPlacer.BuildMode.DESTROY)
        {
            Destroy();
        }
    }

    void Place()
    {
        if(slot.item.itemData == null || playerHotbar.GetActiveSlot() != slot) BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);

        if (Input.GetMouseButtonDown(0))
        {
            if (BuildingPlacer.Instance.PlacingBuildingPart())
            {
                AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "Place", transform.position);
                slot.item.quantity -= 1;
                slot.UpdateUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            BuildingPlacer.Instance.RotatePreview(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            playerHotbar.SetActiveSlot(playerHotbar.activeSlotNum);
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
        }
    }

    void Destroy()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (BuildingPlacer.Instance.DestroyBuildingPart())
            {
                //Success
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
        }
    }

    public void SelectBuildingPart(string buildingName)
    {
        BuildingPlacer.Instance.SelectBuildingPart(BuildingManager.Instance.GetBuildingPartByName(buildingName));
        BuildingPlacer.Instance.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
    }

    public void UpgradePart()
    {
        BuildingPart buildingPart = BuildingPlacer.Instance.GetTargetBuildingPart();
        if (buildingPart == null) return;
        if (buildingPart.GetModelSettings.Models.Count - 1 == buildingPart.GetModelSettings.ModelIndex) return;
        buildingPart.GetModelSettings.ModelIndex += 1;
        AudioManager.Instance.PlayClipDataSound(slot.item.itemData.baseProperties.typeAudio, "Upgrade", transform.position, buildingPart.GetModelSettings.ModelIndex);
        buildingPart.ChangeModel(buildingPart.GetModelSettings.ModelIndex);
    }
}

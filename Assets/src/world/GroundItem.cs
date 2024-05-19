
using UnityEngine;

public class GroundItem : Interactable
{
    public ItemInstance item;

    public override void OnInteract()
    {
        InventoryManager playerInv = FindObjectOfType<InventoryManager>();
        if (playerInv == null) return;
        playerInv.AddItemToPlayerInventory(item);
        Destroy(transform.gameObject);
    }
}

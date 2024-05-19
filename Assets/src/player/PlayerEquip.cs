using System.Collections;
using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    public Transform itemInHand;

    private PlayerAnimations playerAnim;
    private PlayerAttack playerAttack;
    public bool itemEquipped;
    public bool equipping;

    // Initialization
    private void Start()
    {
        playerAnim = FindObjectOfType<PlayerAnimations>();
        playerAttack = FindObjectOfType<PlayerAttack>();
        ClearItemInHand();
    }

    // Set the active item from the hotbar
    public void SetActiveItem(InventorySlot slot)
    {
        if (slot == null || slot.item == null || slot.item.itemData == null) return;
        StartCoroutine(HandleEquip(slot));
    }

    private IEnumerator HandleEquip(InventorySlot slot)
    {
        equipping = true;
        playerAnim.animator.ResetTrigger("Equip");
        playerAnim.StopCurrentAnimation();
        playerAnim.SetItemAnimations(slot.item.itemData.equipableProperties.anims);
        playerAnim.animator.SetTrigger("Equip");
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "Equip", playerAttack.transform.position);

        float totalDuration = 0;
        if(slot.item.itemData.equipableProperties.anims != null)
        {
            totalDuration = playerAnim.GetClipLengthFromOverride(playerAnim.animator, "player_equip");
        }
        else
        {
            Debug.Log(slot.item.itemData.name + " is missing the override animations");
        }
        UpdateItemInHand(slot);
        yield return new WaitForSeconds(totalDuration);
        equipping = false;
    }

    private void UpdateItemInHand(InventorySlot slot)
    {
        if (slot.item == null) return;
        HideAllItems();
        GameObject item = null;
        Transform childType = GetItemChild(slot);
        childType.gameObject.SetActive(true);
        foreach (Transform child in childType)
        {
            if (child.name == slot.item.itemData.name)
            {
                child.gameObject.SetActive(true);
                item = child.gameObject;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            
        }

        if (item == null)
        {
            itemEquipped = false;
            return;
        }
        itemEquipped = true;
        item.SetActive(true);

        switch (slot.item.itemData.baseProperties.type)
        {
            case ItemObject.ItemType.Ranged:
                if (!item.GetComponent<RangedItem>())
                {
                    Debug.Log(item.name + " is missing the RangedItem Component");
                    return;
                }
                item.GetComponent<RangedItem>().slot = slot; 
                break;
            case ItemObject.ItemType.Melee:
                if (!item.GetComponent<MeleeItem>())
                {
                    Debug.Log(item.name + " is missing the MeleeItem Component");
                    return;
                }
                item.GetComponent<MeleeItem>().slot = slot; 
                break;
            case ItemObject.ItemType.Medical:
                if (!item.GetComponent<MedicalItem>())
                {
                    Debug.Log(item.name + " is missing the MedicalItem Component");
                    return;
                }
                item.GetComponent<MedicalItem>().slot = slot;
                break;
        }
    }

    private Transform GetItemChild(InventorySlot slot)
    {
        switch (slot.item.itemData.baseProperties.type)
        {
            case ItemObject.ItemType.Ranged: return itemInHand.Find("Ranged");
            case ItemObject.ItemType.Melee: return itemInHand.Find("Melee");
            case ItemObject.ItemType.Medical: return itemInHand.Find("Medical");
            default: return null;
        }
    }

    private void HideAllItems()
    {
        foreach(Transform child in itemInHand)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ClearItemInHand()
    {
        foreach (Transform child in itemInHand)
        {
            child.gameObject.SetActive(false);
        }
        playerAnim.ResetOverrides();
    }
}

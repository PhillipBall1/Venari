using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeItem : MonoBehaviour
{
    public InventorySlot slot;
    private PlayerAnimations playerAnim;
    private UI_Manager UI;
    private InventoryManager inventory;
    private PlayerEquip playerEquip;
    private float nextAttackTime;

    void Start()
    {
        playerAnim = transform.root.GetComponent<PlayerAnimations>();
        playerEquip = transform.root.GetComponent<PlayerEquip>();
        UI = FindObjectOfType<UI_Manager>();
        inventory = FindObjectOfType<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (UI.currentUIState != UI_Manager.UIState.None && !playerEquip.equipping) return;
        HandleAttack();
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack(slot.item.itemData.equipableProperties.animationDelay));
            
            nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.2f;
        }
    }

    private IEnumerator Attack(float delay)
    {
        playerAnim.AdjustAnimationSpeed(slot.item.itemData.equipableProperties.attackSpeed);
        playerAnim.StartAttack();
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.personalAudio, "Attack", transform.position);
        AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.worldHitSounds, "Rustle", transform.position, 0);

        int layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;
        yield return new WaitForSeconds(delay);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, slot.item.itemData.equipableProperties.reach, layerMask))
        {
            if (hit.collider == null) yield return null;

            string soundType = LayerMask.LayerToName(hit.collider.gameObject.layer);

            playerAnim.AttackHit();
            MeleeHitAudio(soundType, hit.point);
            AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.worldHitSounds, soundType, hit.point);
            ;
            

            if(GetDecalByType(soundType) != null)
            {
                GameObject decal = Instantiate(GetDecalByType(soundType), hit.point, Quaternion.LookRotation(hit.normal));
                decal.transform.SetParent(hit.collider.transform);
                decal.transform.position += decal.transform.forward * 0.01f;
            }
            var damagable = hit.collider.GetComponent<Farmable>();
            if (damagable != null)
            {
                
                if(damagable.type == "Wood")
                {
                    inventory.AddItemToPlayerInventory(new ItemInstance
                    {
                        itemData = AssetManager.Instance.database.GetItemByName("Wood"),
                        quantity = slot.item.itemData.baseProperties.damageRange.treeDamage,
                    });
                    damagable.TakeDamage(slot.item.itemData.baseProperties.damageRange.treeDamage);
                }
                if (damagable.type == "Stone")
                {
                    inventory.AddItemToPlayerInventory(new ItemInstance
                    {
                        itemData = AssetManager.Instance.database.GetItemByName("Stone"),
                        quantity = slot.item.itemData.baseProperties.damageRange.nodeDamage,
                    });
                    damagable.TakeDamage(slot.item.itemData.baseProperties.damageRange.nodeDamage);
                }
            }
        }
        yield return new WaitForSeconds((playerAnim.CalculateClipLength("player_attack") - delay) * slot.item.itemData.equipableProperties.attackSpeed);
        playerAnim.AdjustAnimationSpeed(1);
    }

    private GameObject GetDecalByType(string soundType)
    {
        switch (soundType)
        {
            case "Dirt": return AssetManager.Instance.hitDecals[0];
            case "Wood": return AssetManager.Instance.hitDecals[1];
            case "Stone": return AssetManager.Instance.hitDecals[2];
            case "Metal": return AssetManager.Instance.hitDecals[3];
            default: return null;
        }
    }

    private void MeleeHitAudio(string soundType, Vector3 point)
    {
        if (slot.item.itemData.baseProperties.personalAudio == null) return;
        bool hitAcceptable = false;
        for (int i = 0; i < slot.item.itemData.meleeProperties.acceptableHits.Length; i++)
        {
            if (soundType == slot.item.itemData.meleeProperties.acceptableHits[i].ToString())
            {
                hitAcceptable = true;
                break;
            }
        }

        if (hitAcceptable)
        {
            slot.item.ReduceDurability(1);
            AudioManager.Instance.PlayClipDataSound(slot.item.itemData.baseProperties.personalAudio, "Hit", point, 1);
        }
        else
        {
            slot.item.ReduceDurability(3);
            AudioManager.Instance.PlayClipDataSound(slot.item.itemData.baseProperties.personalAudio, "Hit", point, 0);
        }
        slot.UpdateUI();
    }
}

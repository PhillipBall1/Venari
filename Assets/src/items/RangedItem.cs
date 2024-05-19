using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.LowLevel;

public class RangedItem : MonoBehaviour
{
    public InventorySlot slot;
    private InventoryManager invManager;
    private PlayerAnimations playerAnim;
    private UI_Manager UI;
    private PlayerEquip playerEquip;
    private PlayerLook playerLook;
    private float nextAttackTime;
    private bool reloading = false;
    private bool isAiming = false;
    private bool playOnce = false;

    void Start()
    {
        invManager = FindObjectOfType<InventoryManager>();
        playerAnim = transform.root.GetComponent<PlayerAnimations>();
        playerEquip = transform.root.GetComponent<PlayerEquip>();
        playerLook = Camera.main.GetComponent<PlayerLook>();
        UI = FindObjectOfType<UI_Manager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (UI.currentUIState != UI_Manager.UIState.None && !playerEquip.equipping) return;

        if (Input.GetMouseButton(1))
        {
            if (!playOnce)
            {
                AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.worldHitSounds, "Rustle", playerAnim.transform.position, 0);
                playOnce = true;
            }
            isAiming = true;
            playerLook.ZoomIn();
        }
        else
        {
            if (playOnce)
            {
                AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.worldHitSounds, "Rustle", playerAnim.transform.position, 0);
                playOnce = false;
            }
            isAiming = false;
            playerLook.ZoomOut();
        }
        if (!isAiming)
        {
            HandleHipFire();
        }
        else
        {
            HandleAimedFire();
        }
        playerAnim.animator.SetBool("IsAiming", isAiming);
        HandleReload();
    }

    private void HandleReload()
    {
        if(Input.GetKeyUp(KeyCode.R) && !reloading && slot.item.GetCurrentAmmo() < slot.item.itemData.rangedProperties.magazineSize)
        {
            reloading = true;
            StartCoroutine(Reload(playerAnim.CalculateClipLength("weapon_reload")));
        }
    }

    private IEnumerator Reload(float delay)
    {
        playerAnim.AdjustAnimationSpeed(slot.item.itemData.rangedProperties.reloadTime);

        int missingAmmo = slot.item.itemData.rangedProperties.magazineSize - slot.item.GetCurrentAmmo();
        int remaining = invManager.RemoveItemAmount(slot.item.itemData.rangedProperties.ammo, missingAmmo);
        int amountToAdd = slot.item.itemData.rangedProperties.magazineSize - remaining;
        if (amountToAdd == 0)
        {
            reloading = false;
            yield return null;
        }
        else
        {
            playerAnim.StartReload();
            AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.personalAudio, "Reload", transform.position);
            yield return new WaitForSeconds(delay);
            slot.item.SetCurrentAmmo(amountToAdd);
            slot.UpdateUI();
            reloading = false;
        }
        playerAnim.AdjustAnimationSpeed(1);
    }

    private void HandleHipFire()
    {
        if (reloading) return;
        if (slot.item.itemData.rangedProperties.isAutomatic)
        {
            if (Input.GetMouseButton(0) && Time.time >= nextAttackTime)
            {
                if (slot.item.GetCurrentAmmo() > 0)
                {
                    playerAnim.StartAttack();
                    StartCoroutine(Attack(slot.item.itemData.equipableProperties.animationDelay));
                }
                else
                {
                    AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "NoAmmo", transform.position);
                }

                nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.1f;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
            {
                if (slot.item.GetCurrentAmmo() > 0)
                {
                    playerAnim.StartAttack();
                    StartCoroutine(Attack(slot.item.itemData.equipableProperties.animationDelay));
                }
                else
                {
                    AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "NoAmmo", transform.position);
                }

                nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.1f;
            }
        }
        
    }

    private void HandleAimedFire()
    {
        if (reloading) return;
        if (slot.item.itemData.rangedProperties.isAutomatic)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Time.time >= nextAttackTime)
            {
                if (slot.item.GetCurrentAmmo() > 0)
                {
                    playerAnim.StartAimedAttack();
                    StartCoroutine(Attack(slot.item.itemData.equipableProperties.animationDelay));
                }
                else
                {
                    AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "NoAmmo", transform.position);
                }

                nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.1f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextAttackTime)
            {
                if (slot.item.GetCurrentAmmo() > 0)
                {
                    playerAnim.StartAimedAttack();
                    StartCoroutine(Attack(slot.item.itemData.equipableProperties.animationDelay));
                }
                else
                {
                    AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, "NoAmmo", transform.position);
                }

                nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.1f;
            }
        }
    }

    private IEnumerator Attack(float delay)
    {
        playerAnim.AdjustAnimationSpeed(slot.item.itemData.equipableProperties.attackSpeed);
        
        slot.item.ReduceAmmo(1);
        slot.item.ReduceDurability(1);
        slot.uiSlot.UpdateInstance();
        int layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;
        yield return new WaitForSeconds(delay);
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.personalAudio, "Attack", transform.position);
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, slot.item.itemData.equipableProperties.reach, layerMask))
        {
            if (hit.collider == null) yield return null;

            string soundType = LayerMask.LayerToName(hit.collider.gameObject.layer);
            AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.typeAudio, soundType, hit.point);
            AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.worldHitSounds, soundType, hit.point);
            //var damageable = hit.collider.GetComponent<NPC>();
            //if (damageable != null)
            //{
            //    // Apply damage based on weapon tier and damage range
            //    int damage = CalculateDamage(slot.item.meleeProperties);
            //    damageable.ApplyDamage(damage);
            //}
        }
        playerLook.AddRecoil(new Vector2(-slot.item.itemData.rangedProperties.recoilY, slot.item.itemData.rangedProperties.recoilX));
        yield return new WaitForSeconds((playerAnim.CalculateClipLength("player_attack") - delay) * slot.item.itemData.equipableProperties.attackSpeed);
        playerAnim.AdjustAnimationSpeed(1);
    }
}

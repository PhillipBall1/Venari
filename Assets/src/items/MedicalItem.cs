using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalItem : MonoBehaviour
{
    public InventorySlot slot;
    private PlayerAnimations playerAnim;
    private UI_Manager UI;
    private PlayerEquip playerEquip;
    private PlayerHealth playerHealth;
    private float nextAttackTime;

    void Start()
    {
        playerAnim = transform.root.GetComponent<PlayerAnimations>();
        playerEquip = transform.root.GetComponent<PlayerEquip>();
        playerHealth = transform.root.GetComponent<PlayerHealth>();
        UI = FindObjectOfType<UI_Manager>();
    }
    void Update()
    {
        if (UI.currentUIState != UI_Manager.UIState.None && !playerEquip.equipping) return;
        HandleUse();
    }

    private void HandleUse()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextAttackTime)
        {
            StartCoroutine(Use(playerAnim.CalculateClipLength("player_attack") + 0.2f));

            nextAttackTime = Time.time + playerAnim.CalculateClipLength("player_attack") + 0.2f;
        }
    }

    private IEnumerator Use(float delay)
    {
        playerAnim.StartAttack();
        AudioManager.Instance.PlayClipDataSoundRandom(slot.item.itemData.baseProperties.personalAudio, "Use", transform.position);
        AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.worldHitSounds, "Rustle", transform.position, 0);

        yield return new WaitForSeconds(delay);

        if (slot.item.itemData.medicalProperties.instantRecovery)
        {
            playerHealth.Heal(slot.item.itemData.medicalProperties.healthRecovery);
        }
        else
        {
            playerHealth.Heal(slot.item.itemData.medicalProperties.healthRecovery / 2);
            playerHealth.HealOverTime(slot.item.itemData.medicalProperties.healthRecovery / 2, slot.item.itemData.medicalProperties.recoveryDuration);
        }
        slot.item.quantity -= 1;
        slot.UpdateUI();
    }
}

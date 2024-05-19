using System.Collections;
using System.Drawing;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerAttack : MonoBehaviour
{
    private PlayerAnimations playerAnim;
    private PlayerEquip playerEquip;
    private InventorySlot slot;
    private UI_Manager UI;
    private float nextAttackTime;
    void Start()
    {
        playerAnim = GetComponent<PlayerAnimations>();
        playerEquip = GetComponent<PlayerEquip>();
        UI = FindObjectOfType<UI_Manager>();
    }

    void Update()
    {
        if (UI.currentUIState != UI_Manager.UIState.None && !playerEquip.equipping) return;

        if (Input.GetMouseButton(0) && Time.time >= nextAttackTime && !playerEquip.itemEquipped)
        {
            StartCoroutine(Punch(0.14f));
            nextAttackTime = Time.time + 1.2f;
        }
    }

    private IEnumerator Punch(float delay)
    {
        playerAnim.StartAttack();
        AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.punchAudio, "Attack", transform.position, 0.05f);
        AudioManager.Instance.PlayClipDataSound(AudioManager.Instance.worldHitSounds, "Rustle", Vector3.zero, 0);
        int layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;
        yield return new WaitForSeconds(delay);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, slot?.item == null ? 1.6f : slot.item.itemData.equipableProperties.reach + 1.6f, layerMask))
        {
            if (hit.collider == null) yield return null;

            int hitLayer = hit.collider.gameObject.layer;
            string soundType = LayerMask.LayerToName(hitLayer);
            playerAnim.AttackHit();
            AudioManager.Instance.PlayClipDataSoundRandom(AudioManager.Instance.punchAudio, soundType, transform.position);
        }
        playerAnim.AdjustAnimationSpeed(1);
    }
}

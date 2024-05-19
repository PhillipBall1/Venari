using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerEquipment : MonoBehaviour
{
    public TMP_Text headShotReduction;
    public TMP_Text bodyShotReduction;
    public TMP_Text limbShotReduction;
    public TMP_Text temperatureChange;
    private Resistance resistances;

    private ItemInstance helmet;
    private ItemInstance chest;
    private ItemInstance legs;

    private void Start()
    {
        UpdateStatusText();
    }

    private void UpdateResistances()
    {
        resistances.headShotReduction = helmet != null ? helmet.itemData.armorProperties.resistances.headShotReduction : 0;
        resistances.bodyShotReduction = chest != null ? chest.itemData.armorProperties.resistances.bodyShotReduction : 0;
        resistances.limbShotReduction = legs != null ? legs.itemData.armorProperties.resistances.limbShotReduction : 0;

        resistances.tempChange = (helmet?.itemData.armorProperties.resistances.tempChange ?? 0) +
                                  (chest?.itemData.armorProperties.resistances.tempChange ?? 0) +
                                  (legs?.itemData.armorProperties.resistances.tempChange ?? 0);
    }

    public void EquipGear(ItemInstance item)
    {
        if (item == null) return;
        if (item.itemData.baseProperties.type != ItemObject.ItemType.Armor) return;

        switch (item.itemData.armorProperties.armorType)
        {
            case ItemObject.ArmorType.Head: helmet = item; break;
            case ItemObject.ArmorType.Chest: chest = item; break;
            case ItemObject.ArmorType.Legs: legs = item; break;
        }
        UpdateResistances();
        PlayArmorSound(item);
        UpdateStatusText();
    }

    public void UnequipGear(ItemInstance item)
    {
        if (item == null) return;
        switch (item.itemData.armorProperties.armorType)
        {
            case ItemObject.ArmorType.Head: helmet = null; break;
            case ItemObject.ArmorType.Chest: chest = null; break;
            case ItemObject.ArmorType.Legs: legs = null; break;
        }
        UpdateResistances();
        PlayArmorSound(item);
        UpdateStatusText();
    }

    private void PlayArmorSound(ItemInstance item)
    {
        AudioManager.Instance.PlayClipDataSoundRandom(item.itemData.baseProperties.typeAudio, "Equip", transform.position);
    }

    private void UpdateStatusText()
    {
        //headShotReduction.text = FormatPercentage(resistances.headShotReduction);
        //bodyShotReduction.text = FormatPercentage(resistances.bodyShotReduction);
        //limbShotReduction.text = FormatPercentage(resistances.limbShotReduction);
        //temperatureChange.text = FormatTemperatureChange(resistances.tempChange);
    }

    private string FormatPercentage(float value)
    {
        return (value * 100).ToString("F0") + "%";
    }

    private string FormatTemperatureChange(float tempChange)
    {
        return (tempChange >= 0 ? "+" : "-") + Mathf.Abs(tempChange).ToString();
    }

    public Resistance GetResistances() { return resistances; }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CraftingRecipe;

[CreateAssetMenu(fileName = "New Item", menuName = "System/Items/New Item")]
public class ItemObject : ScriptableObject
{
    #region Structs
    [System.Serializable]
    public struct BaseProperties
    {
        [Header("Required")]
        public Sprite uiImage; // UI Image display of the item inside of UI slots
        public GameObject prefab; // Prefab, generally for when the item is thrown onto the ground
        public string description; // Description used in the tooltip for information on the item
        public int maxStackSize; // Max stacksize for the item inside of a single inventory slot
        [Header("Hidden")]
        public int id; // ID of the item (automatically set)
        public ItemType type; // The type of item, used for a bunch of different sorting methods

        
        public bool equipable; // Whether the item is holdable in the players hands
        public bool hasDurability; // Whether the item has a durability and can break
        public bool hotbarFirst; // Whether the item should be added to the hotbar first on transfers
        public bool craftable; // Whether or not the item can be crafted
        public bool cookable; // Whether or nort the item has a product after being cooked
        public bool doesDamage; // Whether or not we need to care about damaging things, like attackSpeed
        [Header("Damage")]
        public DamageRange damageRange;
        [Header("Audio")]
        public AudioClipData personalAudio;
        public AudioClipData typeAudio;
        public AudioClipData globalAudio;
    }
    [System.Serializable]
    public struct FoodProperties
    {
        public bool isLiquid; // Whether the food is a liquid (affects how it's consumed or stored)
        public int hungerRestore; // Amount of hunger restored upon consumption
        public int thirstRestore; // Amount of thirst quenched upon consumption
        public int healthRestore; // Amount of health restored upon consumption
        public float spoilTime; // Time until the food spoils (if applicable)
        public bool cookable;
        public CookedInfo cookedInfo; // Health restored by the cooked version
        public bool isCooked; // Current state of the food (raw or cooked)
    }
    [System.Serializable]
    public struct MeleeProperties
    {
        public ProperHitLayer[] acceptableHits;
        public bool canBreak; // Whether the weapon can break or degrade with use
    }
    [System.Serializable]
    public struct RangedProperties
    {
        [Header("Required")]
        public ItemObject ammo; // Ammo that is used by this ranged weapon
        public float reloadTime; // Time it takes to reload
        public int magazineSize; // Capacity of ammunition before needing to reload
        public float recoilY;
        public float recoilX;
        public bool isAutomatic; // Whether the weapon fires automatically when the trigger is held
    }
    [System.Serializable]
    public struct ArmorProperties
    {
        [Header("Required")]
        public Resistance resistances; // Specific resistances, like fire, poison, etc.
        [Header("Other")]
        public float weight; // Weight of the armor, affecting mobility or stamina
        public AudioClipData equipSounds; // Sound effect when the armor is equipped
        public ArmorType armorType;
    }
    [System.Serializable]
    public struct MedicalProperties
    {
        [Header("Required")]
        public int healthRecovery; // Amount of health restored

        [Header("Recovery Type")]
        public float recoveryDuration; // Time over which health is restored (for gradual healing)
        public bool instantRecovery; // Whether the health recovery is instant
        public bool isAntidote; // Whether the item acts as an antidote for poisons
    }
    [System.Serializable]
    public struct TrapProperties
    {
        [Header("Required")]
        public TrapType trapType; // Type of trap (e.g., snare, pitfall, explosive)
        public int damage; // Damage dealt by the trap (if applicable)
        public float setupTime; // Time required to set up the trap
        public float triggerRadius; // Radius within which the trap is triggered
        public float triggerDelay; // Delay between trigger and trap activation
        public bool reusable; // Whether the trap can be reused after triggering
        
        [Header("Bait Input")]
        public List<ItemObject> baitItems; // Items that can be used as bait for the trap
    }
    [System.Serializable]
    public struct EquipableProperties
    {
        public int tier; // The tier of the equipable item
        public float reach; // Effective range of the weapon (how far it can hit)
        [Tooltip("Lower = faster")]
        public float attackSpeed; // Determines the speed of the attacks
        [Header("Animations")]
        public float animationDelay;
        public AnimatorOverrideController anims; // Animation for holding the item
        public float soundDelay;
    }
    [System.Serializable]
    public struct AmunitionProperties
    {
        
    }
    [System.Serializable]
    public struct DurabilityProperties
    {
        public Durability durability;
    }
    [System.Serializable]
    public struct ConstructionProperties 
    {
        public float upgradeTime;
    }
    [System.Serializable]
    public struct CraftingProperties
    {
        public List<RecipeItem> recipeItems;
        public int craftingTime;
        public int output;
    }

    [System.Serializable]
    public struct CookableProperties
    {
        public ItemObject product;
        public float cookTime;
        public int cookQuantity;
    }
    #endregion

    public BaseProperties baseProperties;
    public FoodProperties foodProperties;
    public MeleeProperties meleeProperties;
    public RangedProperties rangedProperties;
    public ArmorProperties armorProperties;
    public MedicalProperties medicalProperties;
    public TrapProperties trapProperties;
    public ConstructionProperties constructionProperties;
    public CraftingProperties craftingProperties;
    public EquipableProperties equipableProperties;
    public DurabilityProperties durabilityProperties;
    public AmunitionProperties amunitionProperties;
    public CookableProperties cookableProperties;

    public enum ItemType
    {
        Melee,
        Food,
        Ranged,
        Resource,
        Armor,
        Medical,
        Construction,
        Ammunition,
        Component,
        Trap
    }

    public enum ProperHitLayer
    {
        Dirt,
        Wood,
        Stone,
        Iron,
        Steel,
        Water
    }

    public enum ArmorType
    {
        None,
        Head,
        Chest,
        Legs
    }

    public enum TrapType
    {
        Snare,
        Pitfall,
        Explosive,
        Net,
        Electric
    }
}

[System.Serializable]
public struct DamageRange
{
    public int damageToNPC;
    public int treeDamage;
    public int nodeDamage;
}

[System.Serializable]
public struct CookedInfo
{
    public Sprite cookedSprite;
    public int cookTime;
    public int cookedHungerRestore;
    public int cookedThirstRestore;
    public int cookedHealthRestore;
}


[System.Serializable]
public class Durability
{
    public int maxDurability;
    public int currentDurability;
    public bool repairable;
}

[System.Serializable]
public class RecipeItem
{
    public ItemObject item;
    public int quantity;
}

[System.Serializable]
public struct Resistance
{
    [Range(0f, 1f)]
    public float headShotReduction;
    [Range(0f, 1f)]
    public float bodyShotReduction;
    [Range(0f, 1f)]
    public float limbShotReduction;
    [Range(-10f, 10f)]
    public float tempChange;
}
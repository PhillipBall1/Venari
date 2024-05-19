using System;
using UnityEngine;

[System.Serializable]
public class ItemInstance
{

    public event EventHandler QuantityChanged; // Event to subscribe to for quantity changes

    public ItemObject itemData = null;

    public int _quantity;
    public int quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnQuantityChanged(); // Call method to handle the event.
            }
        }
    }

    public int currentAmmo;

    private Durability durability = new Durability
    {
        maxDurability = 100,
        currentDurability = 100,
        repairable = true,
    };

    // Methods to modify durability
    public void ReduceDurability(int amount)
    {
        durability.currentDurability -= amount;
        durability.currentDurability = Mathf.Max(durability.currentDurability, 0);
    }
    public void ReduceAmmo(int amount)
    {
        currentAmmo -= amount;
        currentAmmo = Mathf.Max(currentAmmo, 0);
    }
    protected virtual void OnQuantityChanged()
    {
        QuantityChanged?.Invoke(this, EventArgs.Empty);
    }


    public Durability GetDurability(){ return durability; }
    public void SetDurability(Durability durability){ this.durability = durability; }

    public int GetCurrentAmmo() { return currentAmmo; }

    public void SetCurrentAmmo(int ammo) { currentAmmo = ammo; }
}
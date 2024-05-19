using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private PlayerEquipment playerGear;

    void Start()
    {
        currentHealth = maxHealth; // Set current health to max at start
        playerGear = GetComponent<PlayerEquipment>();
    }

    public void TakeDamage(int damage, DamageType type)
    {
        switch (type)
        {
            case DamageType.None:
                currentHealth -= damage;
                break;
            case DamageType.Head:
                currentHealth -= (int)(damage * (1 - playerGear.GetResistances().headShotReduction) * 1.5f);
                break;
            case DamageType.Body:
                currentHealth -= (int)(damage * (1 - playerGear.GetResistances().bodyShotReduction));
                break;
            case DamageType.Limb:
                currentHealth -= (int)(damage * (1 - playerGear.GetResistances().limbShotReduction) * 0.75f);
                break;
        }
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't fall below 0

        Debug.Log("Player takes " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't exceed max

        Debug.Log("Player heals " + amount + ". Current health: " + currentHealth);
    }

    public IEnumerator HealOverTime(int amount, float duration)
    {
        float amountPerSecond = amount / duration;
        float timePassed = 0;

        while (timePassed < duration)
        {
            Heal(Mathf.CeilToInt(amountPerSecond * Time.deltaTime));
            timePassed += Time.deltaTime;
            yield return null; // Wait for next frame
        }
    }

    public IEnumerator DamageOverTime(int damage, float duration)
    {
        float damagePerSecond = damage / duration;
        float timePassed = 0;

        while (timePassed < duration)
        {
            TakeDamage(Mathf.CeilToInt(damagePerSecond * Time.deltaTime), DamageType.None);
            timePassed += Time.deltaTime;
            yield return null; // Wait for next frame
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");

        // Handle death here (e.g., respawn, game over screen)
    }
    public int GetPlayerHealth(){return currentHealth;}


    public enum DamageType
    {
        None,
        Head,
        Body,
        Limb
    }
}

using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public float hunger = 300f;
    public float thirst = 250f;
    public float temperature = 37f; // Normal body temperature

    public float hungerDecayRate = 1f;
    public float thirstDecayRate = 0.75f;
    public float temperatureEffectRate = 0.5f;
    public float playerDamageRate = 10f;
    public int playerDamage = 1;
    private bool impactPlayer = true;
    private bool isImpactingPlayer = false;
    private Coroutine impactCoroutine;
    private PlayerHealth playerHealth;
    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Update hunger and thirst over time
        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;

        if (temperature < 35f) // Too cold
        {
            // Implement effects of being too cold (e.g., slower movement)
        }
        else if (temperature > 39f) // Too hot
        {
            // Implement effects of being too hot (e.g., faster thirst decay)
        }

        // Check for critical levels
        if (hunger <= 0 || thirst <= 0)
        {
            if (isImpactingPlayer) return;

            impactCoroutine = StartCoroutine(ImpactPlayer());
        }
        else
        {
            if (!isImpactingPlayer) return;

            StopCoroutine(impactCoroutine);
            isImpactingPlayer = false;
        }
    }

    private IEnumerator ImpactPlayer()
    {
        isImpactingPlayer = true;

        while (hunger <= 0 || thirst <= 0)
        {
            playerHealth.TakeDamage(playerDamage, PlayerHealth.DamageType.Body);
            yield return new WaitForSeconds(playerDamageRate);
        }

        isImpactingPlayer = false;
    }

    public void Eat(float foodValue)
    {
        hunger += foodValue;
        hunger = Mathf.Clamp(hunger, 0, 300f); // Clamp to max hunger value
    }

    public void Drink(float waterValue)
    {
        thirst += waterValue;
        thirst = Mathf.Clamp(thirst, 0, 250f); // Clamp to max thirst value
    }

    public void ChangeTemperature(float temperatureChange)
    {
        temperature += temperatureChange;
        // Optionally, clamp temperature to a range
    }
}

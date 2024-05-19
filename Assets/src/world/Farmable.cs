using System.Collections;
using UnityEngine;

public class Farmable : MonoBehaviour
{
    public string type;
    private float maxHealth;
    private float currentHealth;
    private float respawnTime = 1800f; 

    void Start()
    {
        type = LayerMask.LayerToName(gameObject.layer);
        maxHealth = Random.Range(800, 1001) * transform.localScale.x;
        currentHealth = maxHealth; 
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        //currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            Debug.Log("tree down");
        }
    }

    private void Die()
    {
        // Play tree fall down animation or other visual effect here
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        GetComponent<CapsuleCollider>().enabled = false;
        foreach(Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().enabled = false;
        }
        yield return new WaitForSeconds(respawnTime);
        GetComponent<CapsuleCollider>().enabled = true;
        foreach (Transform child in transform)
        {
            child.GetComponent<MeshRenderer>().enabled = true;
        }
        currentHealth = maxHealth;
    }
}

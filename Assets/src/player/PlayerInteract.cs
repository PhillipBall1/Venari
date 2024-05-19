using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactionDistance = 5f; // The distance within which the player can interact

    public void Interact()
    {
        RaycastHit hit;

        // Cast a ray from the camera forward
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, interactionDistance))
        {
            // Check if the object hit is interactable
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }
        }
    }
}

// Interactable base class
public abstract class Interactable : MonoBehaviour
{
    public abstract void OnInteract(); // This method is called when the player interacts with the object
}

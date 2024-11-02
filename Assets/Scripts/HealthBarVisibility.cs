using UnityEngine;

public class HealthBarVisibility : MonoBehaviour
{
    public GameObject healthBarCanvas; // Reference to the Canvas holding the health bar UI

    private void Start()
    {
        // Initially hide the health bar
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player has entered the trigger area
        if (other.CompareTag("Player"))
        {
            if (healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(true); // Show the health bar
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player has exited the trigger area
        if (other.CompareTag("Player"))
        {
            if (healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(false); // Hide the health bar
            }
        }
    }
}

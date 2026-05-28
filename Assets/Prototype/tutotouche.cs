using UnityEngine;

public class tutotouche : MonoBehaviour
{
    private bool playerInRange = false;
    [SerializeField] private GameObject interactPrompt; // The floating text object

    private void Start()
    {
        // Ensure the text is hidden when the game start
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            // Show text when the player is in th zone
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;

            // Hide the text when quitting the zone
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }


}

using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue à lancer")]
    public DialogueData dialogue; // game designer glisse son fichier dialogue ici

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            // optionnel : affiche un petit bouton "Appuyer sur E pour parler"
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Si le joueur est dans la zone et qu'il appuie sur E
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) 
        {
            DialogueManager.Instance.StartDialogue(dialogue);
            // optionnel : desctiver ce trigger si on ne veut parler qu'une fois
            gameObject.SetActive(false);
        }
    }
}

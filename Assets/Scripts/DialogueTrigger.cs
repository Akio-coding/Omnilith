using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue à lancer")]
    public DialogueData dialogue; // game designer slides his dialogue file here 

    [Header("UI Element")]
    [SerializeField] private GameObject interactPrompt; // The floating text object

    private bool playerInRange = false;
    [SerializeField] private bool destroyDialogueAfterUtilisation = false;
    [SerializeField] private bool isDoableMoreThan1Time = false;

    // Memory variable to check if the dialogue was open a fraction of a second ago
    private bool wasDialogueOpenLastFrame = false;

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
            if(interactPrompt != null)
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
            if(interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    // LateUpdate runs at the very end of every frame, after Update()
    private void LateUpdate()
    {
        // We record the state of the dialogue panel at the end of the frame
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel != null)
        {
            wasDialogueOpenLastFrame = DialogueManager.Instance.dialoguePanel.activeInHierarchy;
        }
    }

    private void Update()
    {
        // If the player is in the zone and interact with E
        if (playerInRange && Input.GetButtonDown("Interact") && !DialogueManager.Instance.dialoguePanel.activeInHierarchy && !wasDialogueOpenLastFrame) 
        {
            DialogueManager.Instance.StartDialogue(dialogue);

            // Hide the text when we interact
            if(interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            if (destroyDialogueAfterUtilisation) 
            {
                // optionnel : desctiver ce trigger si on ne veut parler plus d'une fois
                this.enabled = (false);
            }
        }
    }
}

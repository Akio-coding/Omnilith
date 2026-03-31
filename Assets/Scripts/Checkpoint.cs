using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite activeSprite; // Image du checkpoint activé (drapeau levé)
    [SerializeField] private Sprite inactiveSprite; // Image inactive

    private SpriteRenderer sr;
    private bool isActivated = false;
    private bool playerInRange = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) 
        {
            // Si le joueur touche le checkpoint et qu'il n'est pas déjà le point actif
            if (!isActivated)
            {
                ActivateCheckpoint();
            }

            else
            {
                SetCheckpoint();
            }

            StartCoroutine(WaitDialogueAndOpenMenu());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = false;
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // 1. On change le visuel
        if (sr != null && activeSprite != null)
        {
            sr.sprite = activeSprite;
        }

        // 2. On prévient le chef (GameManager)
        GameManager.instance.UpdateCheckpoint(transform.position);
    }

    private void SetCheckpoint()
    {
        GameManager.instance.UpdateCheckpoint(transform.position);
    }

    // La Coroutine qui attend la fin du dialogue
    private IEnumerator WaitDialogueAndOpenMenu()
    {
        // 1. On attend la fin de l'image (frame) actuelle pour être sûr que DialogueTrigger ait eu le temps de s'activer
        yield return new WaitForEndOfFrame();

        // 2. Si un panneau de dialogue est affiché à l'écran...
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguePanel.activeInHierarchy)
        {
            // ... On met ce script en pause et on boucle tant que le dialogue est ouvert
            while (DialogueManager.Instance.dialoguePanel.activeInHierarchy)
            {
                yield return null;
            }
        }

        // 3. Une fois le dialogue terminé (ou s'il n'y en avait pas), on vérifie que le joueur n'est pas parti loin, puis on ouvre le menu !
        if (MenuManager.Instance != null && playerInRange)
        {
            MenuManager.Instance.OpenMenu();
        }
    }
}

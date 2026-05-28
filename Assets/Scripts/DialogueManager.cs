using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public static event Action OnMonkeyUnlocked; // l'alarme pour le singe !
    
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerPortrait;

    [Header("Réglages")]
    public float typingSpeed = 0.04f; // vitesse d'affichage des lettres 

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // si le dialogue est ouvert et qu'on appuie sur 'e' 
        if (dialoguePanel.activeInHierarchy && Input.GetButtonDown("Interact"))
        {
            DisplayNextLine();
        }
    }

    public void StartDialogue(DialogueData newDialogue)
    {
        currentDialogue = newDialogue;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        DisplayNextLine();

        // optionnel bloquer les mouvements du joueur
    }

    public void DisplayNextLine()
    {
        if (isTyping)
        {
            // Si on interagit pendant que ça tape, on affiche tout d'un coup
            StopAllCoroutines();
            dialogueText.text = currentDialogue.lines[currentLineIndex - 1].texte;
            isTyping = false;
            return;
        }

        if(currentLineIndex < currentDialogue.lines.Length)
        {
            StartCoroutine(TypeSentence(currentDialogue.lines[currentLineIndex]));
            currentLineIndex++;
        }

        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeSentence(DialogueData.DialogueLine line)
    {
        isTyping = true;
        nameText.text = line.speakerName;
        dialogueText.text = "";

        // --- GESTION DE L'IMAGE ---
        // On vérifie si la ligne de dialogue possède un sprite pour la tête
        if(line.speakerHead != null)
        {
            speakerPortrait.sprite = line.speakerHead;
            speakerPortrait.gameObject.SetActive(true);
        }
        else
        {
            // S'il n'y a pas de tête de renseignée, on cache le carré blanc
            speakerPortrait.gameObject.SetActive(false);
        }

        // Effet machine à écrire
        foreach (char letter in line.texte.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // On vérifie si on doit débloquer le singe
        if (currentDialogue.unlocksMonkey)
        {
            //  On déclenche l'alarme que le singe va écouter
            OnMonkeyUnlocked?.Invoke();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneUIConnector : MonoBehaviour
{
    [Header("--- Pour le Menu Manager ---")]
    public GameObject checkpointMenuPanel;
    public Button healButton;
    public Button leaveButton;

    [Header("--- Pour le Dialogue Manager ---")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image speakerPortrait;

    void Start()
    {
        // On attend un tout petit peu pour s'assurer que le Game Manager est bien là
        Invoke(nameof(ConnectUIToManagers), 0.05f);
    }

    void ConnectUIToManagers()
    {
        MenuManager menu = null;

        // 1. On cherche le MenuManager
        if (MenuManager.Instance != null)
        {
            menu = MenuManager.Instance;
        }
        else if (DialogueManager.Instance != null)
        {
            menu = DialogueManager.Instance.GetComponent<MenuManager>();
        }

        // 2. Si on a trouvé le MenuManager, on lui donne les boutons et on configure les clics
        if (menu != null)
        {
            menu.menuPanel = checkpointMenuPanel;
            menu.healButton = healButton;
            menu.leaveButton = leaveButton;

            // Configuration du bouton Heal (exécute HealPlayer)
            if (healButton != null)
            {
                healButton.onClick.RemoveAllListeners();
                healButton.onClick.AddListener(() => menu.SendMessage("HealPlayer", SendMessageOptions.DontRequireReceiver));
            }

            // Configuration du bouton Leave (exécute CloseMenu)
            if (leaveButton != null)
            {
                leaveButton.onClick.RemoveAllListeners();
                // CORRECTION ICI : C'est bien leaveButton qui reçoit l'écouteur maintenant !
                leaveButton.onClick.AddListener(() => menu.SendMessage("CloseMenu", SendMessageOptions.DontRequireReceiver));
            }
        }

        // 3. Connexion au DialogueManager
        if (DialogueManager.Instance != null)
        {
            DialogueManager dm = DialogueManager.Instance;
            dm.dialoguePanel = dialoguePanel;
            dm.nameText = nameText;
            dm.dialogueText = dialogueText;
            dm.speakerPortrait = speakerPortrait;

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }

        Debug.Log("<color=green>Succès :</color> Boutons dissociés et configurés correctement !");
    }
}
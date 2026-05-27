using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [Header("UI Elements")]
    [SerializeField] public GameObject menuPanel;
    [SerializeField] public Button healButton;
    [SerializeField] public Button leaveButton;

    private void Awake()
    {
        // Set up a simple Singleton to access this script from anywhere
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Make sure the menu is closed at the start
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        // Tell the buttons what functions to trigger when clicked
        healButton.onClick.AddListener(HealPlayer);
        leaveButton.onClick.AddListener(CloseMenu);
    }

    public void OpenMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        // Pause the game here because enemies can still attack you
        Time.timeScale = 0f; 
    }

    public void CloseMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        // Unpause the game because you paused it
        Time.timeScale = 1f;
    }

    private void HealPlayer()
    {
        // 1. Find the player automatically using the tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            // 2. On essaie de prendre le composant Health
            Health playerHealth = p.GetComponent<Health>();

            // 3. Si on l'a trouvé, c'est notre vrai personnage !
            if (playerHealth != null)
            {
                playerHealth.Heal(playerHealth.maxHealth);
                Debug.Log("Player fully healed!");
                return; // On a fini notre travail, on quitte la fonction
            }
        }

        Debug.LogWarning("Aucun script 'Health' n'a été trouvé sur les objets possédant le tag 'Player'.");
        
        // Note: The menu stays open here as requested. 
        // The player must click the Leave button to call CloseMenu().
    }
}

using UnityEngine;
using UnityEngine.UI; // Indispensable pour manipuler les éléments d'interface

public class HealthBarUI : MonoBehaviour
{
    [Header("Composants")]
    [Tooltip("Glisse ici ton Joueur qui possède le script Health")]
    [SerializeField] private Health playerHealth;

    [Tooltip("Glisse ici l'image HealthBar_Fill de ton Canvas")]
    [SerializeField] private Image fillImage;

    void OnEnable()
    {
        // On s'abonne à l'événement de ton script Health
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthBar;

            // On force l'affichage correct au tout début du jeu
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    void Start()
    {
        if (playerHealth != null)
        {
            // Comme Start s'exécute après tous les Awake(), 
            // on est certain que le joueur a eu le temps de mettre sa vie au maximum !
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.maxHealth);
        }
    }

    void OnDisable()
    {
        // On se désabonne quand l'objet est détruit pour éviter les fuites de mémoire
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    // Cette fonction est appelée automatiquement à chaque fois que le joueur prend un coup ou se soigne !
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (fillImage != null)
        {
            // "fillAmount" demande une valeur entre 0 (vide) et 1 (plein). 
            // On divise la vie actuelle par la vie max (ex: 3 / 5 = 0.6)
            // On doit forcer la conversion en (float) sinon Unity arrondit à 0 !
            fillImage.fillAmount = (float)currentHealth / (float)maxHealth;
        }
    }
}
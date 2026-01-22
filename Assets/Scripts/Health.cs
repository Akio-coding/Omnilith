using System; // Nécessaire pour les Actions (Events)
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int _maxHealth = 5;
    public int maxHealth => _maxHealth;
    [SerializeField] private float iFramesDuration = 1f; // Invincibility duration
    [SerializeField] private int flashCount = 5; // How many flash ?

    // Public property to read private entry (usefull to other scripts to read the privates variables)
    public int CurrentHealth { get; private set; }

    // State
    private bool isInvincible = false;
    private SpriteRenderer sr;
    private Material originalMaterial;

    // EVENTS : Pro section. 
    // Other scripts can subscribe to these events without Health being aware of them
    // It is similar to delegates in unreal 
    // D'autres scripts peuvent s'abonner à ces événements sans que Health ne les connaisse.
    public event Action<int, int> OnHealthChanged; // Envoie (VieActuelle, VieMax)
    public event Action OnDeath;
    public event Action OnDamageTaken; // Utile pour jouer un son ou une anim

    [SerializeField] private Color flashing;
    void Awake()
    {
        CurrentHealth = _maxHealth;
        sr = GetComponent<SpriteRenderer>();
        
    }

    public void TakeDamage(int damageAmount)
    {
        // 1. If we are invincible or dead, ignore 
        if (isInvincible || CurrentHealth <= 0) 
        {
            return;
        }

        // 2. We apply damages
        CurrentHealth -= damageAmount;

        // 3. Notify everyone (UI, Audio, etc.)
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke();

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
        else
        {
            // 4. If we survive, launch the coroutine 
            StartCoroutine(InvincibilityRoutine());
        }
        Debug.Log(CurrentHealth);
    }

    public void Heal(int healAmount)
    {
        CurrentHealth += healAmount;
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log(gameObject.name + " est mort !");

        // Pour le joueur, on relance le niveau ou on affiche Game Over
        // Pour un ennemi : Destroy(gameObject);
    }

    // Coroutine to handle the flashing and the invicibility 
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // flashing effect
        for (int loopCount = 0; loopCount < flashCount; loopCount++)
        {
            sr.color = flashing; // choosed color
            yield return new WaitForSeconds(iFramesDuration / (flashCount * 2));
            sr.color = Color.white; // Normal
            yield return new WaitForSeconds(iFramesDuration / (flashCount * 2));
        }

        sr.color = Color.white;
        isInvincible = false;
        
    }
}

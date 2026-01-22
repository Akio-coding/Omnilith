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

    [Header("Respawn")]
    [SerializeField] private float deathDelay = 2f; // Temps de l'animation de mort
    [SerializeField] private Behaviour[] componentsToDisable; // Scripts à couper (ex: PlayerJump)

    private Animator anim;

    private Rigidbody2D rb;

    [SerializeField] private Collider2D playerCollider;


    void Awake()
    {
        CurrentHealth = _maxHealth;
        sr = GetComponent<SpriteRenderer>();
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // 4. If we survive, launch the coroutine 
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // On désactive le script de mouvement (PlayerJump) et le Collider pour ne plus être touché
        foreach (var component in componentsToDisable)
        {
            component.enabled = false;
        }

        // On joue l'animation et on coupe les contrôles
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetTrigger("Die");
        }
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Stop net
            rb.simulated = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Devient intouchable/fantomatique
        }
        

        // 2. On attend la fin de l'animation
        yield return new WaitForSeconds(deathDelay);

        // 3. LE RESPAWN (Téléportation)
        if (GameManager.instance != null)
        {
            transform.position = GameManager.instance.respawnPoint;
        }
        else
        {
            // Si tu as oublié de mettre le GameManager dans la scène, on le verra ici !
            Debug.LogError("OUPS : Le GameManager est introuvable dans la scène ! Le respawn a échoué.");
        }
        // 4. On remet tout à neuf
        Respawn();
    }

    private void Respawn()
    {
        // Remettre la vie au max
        CurrentHealth = _maxHealth; // (Utilise ta variable maxHealth ou _maxHealth)
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

        // Réactiver les contrôles et la physique
        foreach (var component in componentsToDisable)
        {
            component.enabled = true;
        }

        if (rb != null)
        {
            rb.simulated = true;
        }

        // On réactive le collider via la variable
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Reset Anim
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetTrigger("Respawn");
        }
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

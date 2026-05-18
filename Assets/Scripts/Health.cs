using System; // Nécessaire pour les Actions (Events)
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Correspond a la hurtbox physique du joueur, qui se désactive quand on prends des dégats")]
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private int _maxHealth = 5;
    public int maxHealth => _maxHealth;
    // Public property to read private entry (usefull to other scripts to read the privates variables)
    public int CurrentHealth { get; private set; }

    [Header("Invincibility Settings")]
    [Tooltip("Durée de l'invincibilité en secondes")]
    [SerializeField] private float invincibilityDuration = 1f; // Invincibility duration
    [Tooltip("Combien de fois le joueur clignotte pendant l'invincibilité")]
    [SerializeField] private int flashCount = 5; // How many flash ?
    [Tooltip("couleur dans laquelle il clignotte")]
    [SerializeField] private Color flashing;
    private bool isInvincible = false; // The state of the player

    [Header("Respawn")]
    [Tooltip("Durée pendant laquelle on désactive tous les contrôles du joueur, doit être de la même durée que l'animation de mort en secondes")]
    [SerializeField] private float deathDelay = 1f; // Duration of death animation
    [SerializeField] private Behaviour[] componentsToDisable;

    [Header("Knockback Settings")]
    [Tooltip("La force du recul. X = projection horizontale, Y = petit saut en l'air")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(5f, 2f);
    [Tooltip("Durée de la perte de contrôle pendant le recul")]
    [SerializeField] private float knockbackDuration = 0.2f;

    // ---- Events ----
    // Other scripts can subscribe to these events without Health being aware of them
    // It is similar to delegates in unreal 
    public event Action<int, int> OnHealthChanged; // Send (CurrentHealth, MaxHealth)
    public event Action OnDeath;
    public event Action OnDamageTaken; // Usefull to play a sound or an animation small key board 

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Awake()
    {
        CurrentHealth = _maxHealth;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damageAmount, Transform damageSource = null)
    {
        // If we are invincible or dead, ignore 
        if (isInvincible || CurrentHealth <= 0) 
        {
            return;
        }

        // We apply damages
        CurrentHealth -= damageAmount;

        // Notify everyone (UI, Audio, etc.)
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke();

        if (CurrentHealth <= 0)
        {
            if (CompareTag("Player") == true)
            {
                CurrentHealth = 0;
                StartCoroutine(RespawnRoutine());
            }
            else if (CompareTag("Enemy") == true)
            {
                Die();
            }
        }
        else
        {
            // On applique le knockback si on sait d'où vient le coup
            if (damageSource != null && rb != null)
            {
                StartCoroutine(KnockbackRoutine(damageSource));
            }

            // If we survive, launch the coroutine 
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // La coroutine qui gère la physique du recul
    private IEnumerator KnockbackRoutine(Transform damageSource)
    {
        // 1. On coupe les contrôles (PlayerMovement) pour qu'il n'annule pas la physique
        foreach (var component in componentsToDisable)
        {
            component.enabled = false;
        }

        // 2. On détermine la direction (1 = vers la droite, -1 = vers la gauche)
        float pushDirection = 1f;
        if (transform.position.x < damageSource.position.x)
        {
            pushDirection = -1f; // L'attaquant est à droite, on recule vers la gauche
        }

        // 3. On applique la force !
        rb.velocity = Vector2.zero; // On stoppe le mouvement actuel net
        rb.AddForce(new Vector2(knockbackForce.x * pushDirection, knockbackForce.y), ForceMode2D.Impulse);

        // 4. On attend que le joueur finisse de reculer
        yield return new WaitForSeconds(knockbackDuration);

        // 5. On rend les contrôles
        foreach (var component in componentsToDisable)
        {
            component.enabled = true;
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // Disable the PlayerMovement script and the collider so that wa can no longer be hit
        foreach (var component in componentsToDisable)
        {
            component.enabled = false;
        }

        // Play the animation and cut off the controls
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetTrigger("Die");
        }
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Stop 
            rb.simulated = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Become untouchable/ghostly 
        }
        

        // Waiting for the the animation to end 
        yield return new WaitForSeconds(deathDelay);

        // Respawn (TP)
        if (GameManager.instance != null)
        {
            transform.position = GameManager.instance.respawnPoint;
        }
        else
        {
            Debug.LogError("OUPS : Le GameManager est introuvable dans la scène ! Le respawn a échoué.");
        }
        // Reset all to normal
        Respawn();
    }

    private void Respawn()
    {
        // Set health back to maxHealth
        CurrentHealth = _maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

        // Reactivate controls and physics
        foreach (var component in componentsToDisable)
        {
            component.enabled = true;
        }

        if (rb != null)
        {
            rb.simulated = true;
        }

        // Reactivate the collider 
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        // Reset the animation
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
        if (sr != null)
        {
            for (int loopCount = 0; loopCount < flashCount; loopCount++)
            {
                sr.color = flashing;
                yield return new WaitForSeconds(invincibilityDuration / (flashCount * 2));
                sr.color = Color.white;
                yield return new WaitForSeconds(invincibilityDuration / (flashCount * 2));
            }
            sr.color = Color.white;
        }
        else
        {
            // Si on n'a pas de SpriteRenderer, on attend juste la durée de l'invincibilité normalement
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("meurt meurt meurt");

        // 1. On lance l'animation de KO
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. On désactive la hitbox pour qu'il ne blesse plus le joueur en tombant
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 3. On coupe le moteur physique pour qu'il s'arrête de glisser
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 4. On détruit l'objet après un délai de 1 seconde (laisse le temps à l'anim de se jouer)
        Destroy(this.gameObject, 2f);
    }
}

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

    [Header("Respawn Player")]
    [Tooltip("Durée pendant laquelle on désactive tous les contrôles du joueur, doit être de la même durée que l'animation de mort en secondes")]
    [SerializeField] private float deathDelay = 2f; // Duration of death animation
    [SerializeField] private Behaviour[] componentsToDisable;

    // Réglages personnalisables par ennemi !
    [Header("Death Settings (Enemies)")]
    [Tooltip("Désactiver la physique à la mort ? (Coché pour l'Oignon, Décoché pour le Crabe)")]
    [SerializeField] private bool disablePhysicsOnDeath = true;
    [Tooltip("Détruire l'objet après sa mort ? (Coché pour l'Oignon, Décoché pour le Crabe)")]
    [SerializeField] private bool destroyOnDeath = true;
    [Tooltip("Délai avant destruction (si activé)")]
    [SerializeField] private float destroyDelay = 2f;

    // Option pour désactiver le trigger de mort automatique !
    [Tooltip("Lancer le Trigger 'Die' automatiquement à 0 PV ? (Coché pour l'Oignon, Décoché pour le Crabe)")]
    [SerializeField] private bool triggerDieOnDeath = true;

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

        anim.SetTrigger("TakeDamage");

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
            else if (CompareTag("Enemy") || CompareTag("Ennemy"))
            {
                die();
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

        // On force l'arrêt total pour éviter que les ennemis sans gravité ne glissent à l'infini !
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

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
        if (sr  != null)
        {
            // flashing effect
            for (int loopCount = 0; loopCount < flashCount; loopCount++)
            {
                sr.color = flashing; // Choosed color
                yield return new WaitForSeconds(invincibilityDuration / (flashCount * 2));
                sr.color = Color.white; // Normal
                yield return new WaitForSeconds(invincibilityDuration / (flashCount * 2));
            }

            sr.color = Color.white;
        }
        
        isInvincible = false;
        
    }

    void die() 
    {
        // 1. On lance l'événement pour prévenir le script du Crabe qu'il doit se cacher
        OnDeath?.Invoke();

        // 2. On lance l'animation (Onion Ko, Crab Hide, etc.)
        if (triggerDieOnDeath && anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetTrigger("Die");
        }

        // 3. On désactive la hitbox pour qu'il ne bloque plus le joueur
        if(playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 4. Si c'est un Oignon, il s'arrête de bouger et n'a plus de physique 
        // Si c'est un Crabe, on garde la physique pour sa carapace
        if(disablePhysicsOnDeath && rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 5. Destruction au bout de 2s (Oui pour l'Oignon, Non pour le Crabe)
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}

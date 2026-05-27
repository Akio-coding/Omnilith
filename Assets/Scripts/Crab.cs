using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Crab : MonoBehaviour
{
    private enum State { Patrol, Pause, Chase, Hiding }
    private State currentState = State.Patrol;

    [Header("Patrouille")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float patrolSpeed = 2f;
    [Tooltip("Temps de pause quand arrivé sur un point")]
    [SerializeField] private float pauseDuration = 1.5f;

    private Transform currentPatrolTargert;
    private float pauseTimer;

    [Header("Poursuit")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float visionDistance = 5f;
    private Transform playerTransform;

    [Header("Carapace (Vulnérabilité)")]
    [Tooltip("Temps en secondes avant qu'il ne ressorte de sa carapace")]
    [SerializeField] private float hidingDuration = 6f;
    [Tooltip("Force du recul quand le joueur tape la carapace à l'épée")]
    [SerializeField] private Vector2 carapaceKnockback = new Vector2(6f, 3f);
    [Tooltip("Dégâts infligés aux autres ennemis si la carapace est lancée dessus")]
    [SerializeField] private int thrownDamage = 3;

    private float hidingTimer;
    private bool isCarried = false; // Porté par le singe
    private bool isThrown = false;  // Lancé en l'air par le singe

    [Header("Detection")]
    [SerializeField] private Vector2 visionBoxSize = new Vector2(0.5f,1f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private Transform wallCheck;

    private Rigidbody2D rb;
    private Animator anim;
    private Health health;
    private DamageDealer damageDealer;
    private Collider2D crabCollider;

    private int facingDirection = -1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
        damageDealer = GetComponent<DamageDealer>();

        currentPatrolTargert = pointB;
        UpdateFacingDirection(currentPatrolTargert.position);

        if (health != null)
        {
            health.OnDeath += EnterHidingState;
            health.OnDamageTaken += PlayDamageAnimation;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= EnterHidingState;
            health.OnDamageTaken -= PlayDamageAnimation;
        }
    }

    private void PlayDamageAnimation()
    {
        if (anim != null && currentState != State.Hiding)
        {
            anim.SetTrigger("Damage");
        }
    }

    private void Update()
    {
        if(currentState == State.Hiding)
        {
            HidingBehavior();
            return;
        }

        switch(currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                CheckForPlayer();
                break;

            case State.Pause:
                PauseBehavior();
                CheckForPlayer();
                break;

            case State.Chase:
                ChaseBehavior();
                break;
        }

        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if(anim == null)
        {
            return;
        }
        anim.SetBool("IsWalking", currentState == State.Patrol || currentState == State.Chase);
    }

    // ------ BEHAVIORS ------

    private void PatrolBehavior()
    {
        if (IsObstacleAhead())
        {
            StartPause();
            return;
        }

        rb.velocity = new Vector2(patrolSpeed * facingDirection, rb.velocity.y);

        if (Mathf.Abs(transform.position.x - currentPatrolTargert.position.x) < 0.1f)
        {
            StartPause();
        }
    }

    private void StartPause()
    {
        currentState = State.Pause;
        pauseTimer = pauseDuration;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    private void PauseBehavior()
    {
        pauseTimer -= Time.deltaTime;
        if(pauseTimer <= 0)
        {
            currentPatrolTargert = (currentPatrolTargert == pointA) ? pointB : pointA;
            UpdateFacingDirection(currentPatrolTargert.position);
            currentState = State.Patrol;
        }
    }

    private void ChaseBehavior()
    {
        if(playerTransform == null)
        {
            currentState = State.Patrol;
            return;
        }

        if (IsObstacleAhead())
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = State.Pause;
            pauseTimer = 1f;
            return;
        }

        UpdateFacingDirection(playerTransform.position);

        rb.velocity = new Vector2(chaseSpeed * facingDirection, rb.velocity.y);

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if( distanceToPlayer > visionDistance * 1.5f)
        {
            currentState = State.Patrol; 
            currentPatrolTargert = (transform.position.x > pointA.position.x) ? pointA : pointB;
            UpdateFacingDirection(currentPatrolTargert.position);
        }
    }

    // ------ DETECTION ------

    private void CheckForPlayer()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, visionBoxSize, 0f, Vector2.right * facingDirection, visionDistance, playerLayer);

        if(hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerTransform = hit.collider.transform;
            currentState = State.Chase;
        }
    }

    private bool IsObstacleAhead()
    {
        bool isNearEdge = !Physics2D.Raycast(edgeCheck.position, Vector2.down, 1f, groundLayer);
        bool hittingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * facingDirection, 0.5f, groundLayer);

        return isNearEdge || hittingWall;
    }

    private void UpdateFacingDirection(Vector3 targetPos)
    {
        if(targetPos.x > transform.position.x)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // ------ SPECIAL STATES (DEATH / VULNERABLE STANCE) ------

    private void EnterHidingState()
    {
        currentState = State.Hiding;
        hidingTimer = hidingDuration;
        isCarried = false;
        isThrown = false;
        anim.SetTrigger("VulnerableStance");

        rb.velocity = new Vector2(0, rb.velocity.y);

        if (damageDealer != null)
        {
            damageDealer.enabled = false;
        }

        // On le passe sur le Layer "Throwable" pour que le singe puisse le voir !
        gameObject.layer = LayerMask.NameToLayer("Throwable");
    }

    private void HidingBehavior()
    {
        // Si le singe le porte ou s'il est en train de voler suite à un lancer, on met le timer en pause !
        if(!isCarried && !isThrown)
        {
            hidingTimer -= Time.deltaTime;
            if (hidingTimer < 0)
            {
                WakeUp();
            }
        }
    }

    private void WakeUp()
    {
        if (health != null)
        {
            health.Heal(health.maxHealth);
        }
        if (damageDealer != null)
        {
            damageDealer.enabled = true;
        }
        if (anim != null)
        {
            anim.SetTrigger("WakeUp");
        }
        // Il redevient un ennemi normal, le singe ne peut plus le porter
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        currentState = State.Patrol;
    }

    // --- FONCTIONS POUR LE SINGE ---

    // Fonction à appeler depuis le script du Singe quand il ramasse le crabe
    public void PickUpByMonkey()
    {
        if(currentState != State.Hiding)
        {
            return;
        }
        isCarried = true;
        isThrown = false;
    }

    // Fonction à appeler depuis le script du Singe quand il lance le crabe
    public void ThrowByMonkey()
    {
        if(currentState != State.Hiding)
        {
            return;    
        }
        isCarried = false;
        isThrown = true;
    }

    public void DropByMonkey()
    {
        if( currentState != State.Hiding)
        {
            return;
        }
        isCarried = false;
        isThrown = false;
    }

    // --- GESTION DES COLLISIONS EN CARAPACE ---

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // 1. DÉTECTION DU COUP D'ÉPÉE (Knockback)
        // L'épée du joueur a le script DamageDealer. On vérifie si c'est bien l'épée qui nous touche.
        if(currentState == State.Hiding && collider.GetComponent<DamageDealer>() != null && !isCarried)
        {
            // On calcule d'où vient le coup pour le repousser dans la bonne direction
            float pushDirection = (collider.transform.position.x > transform.position.x) ? -1f : 1f;

            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(carapaceKnockback.x * pushDirection, carapaceKnockback.y), ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Hiding)
        {
            // 2. LE CRABE EST LANCÉ ET TOUCHE UN ENNEMI
            if (isThrown && collision.gameObject.CompareTag("Enemy"))
            {
                Health enemyHealth = collision.gameObject.GetComponent<Health>();

                // On vérifie qu'il ne se blesse pas lui-même
                if (enemyHealth != null && enemyHealth.gameObject != this.gameObject)
                {
                    enemyHealth.TakeDamage(thrownDamage, transform); // Inflict heavy damage
                    DiePermanently(); 
                }
            }

            // 3. LA CARAPACE TOUCHE UN LEVIER
            if (collision.gameObject.CompareTag("Lever"))
            {
                DiePermanently();
            }

            // 4. DÉTECTION DU SOL (Fin du lancer)
            // Si la carapace lancée touche le sol (GroundLayer), elle n'est plus considérée comme "en l'air"
            if (isThrown &&((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                isThrown = false;
            }
        }
    }

    private void DiePermanently()
    {
        anim.SetTrigger("Die");
        // Optionnel : Jouer un effet de particule de destruction ou un son ici
        Destroy(gameObject, 1f);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 boxCenter = (Vector2)transform.position + (Vector2.right * facingDirection * (visionDistance / 2f));
        Gizmos.DrawWireCube(boxCenter, new Vector2(visionDistance, visionBoxSize.y));

        if (edgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(edgeCheck.position, Vector2.down * 1f);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(wallCheck.position, Vector2.right * facingDirection * 0.5f);
        }
    }
}

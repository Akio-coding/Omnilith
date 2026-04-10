using UnityEngine;

public class Onion : MonoBehaviour
{
    private enum State { Patrol, Warning, Charging, Recovering }
    private State currentState = State.Patrol;

    [Header("Stats")]
    [SerializeField] private int damage = 1;

    [Header("Patrouille")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float patrolSpeed = 2f;
    private Transform currentPatrolTarget;

    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float warningDuration = 0.5f; // Temps d'arrêt avant de foncer
    [SerializeField] private float chargeMaxDuration = 1.5f; // Temps de course si le joueur esquive
    [SerializeField] private float recoverDuration = 1f; // Temps de pause à la fin de la charge

    [Header("Détection")]
    [SerializeField] private float visionDistance = 6f; 
    [SerializeField] private Vector2 visionBoxSize = new Vector2(0.5f, 1.5f);
    [SerializeField] private LayerMask playerLayer; // Layer du joueur pour la vision
    [SerializeField] private LayerMask groundLayer; // Layer du sol pour éviter de tomber
    [SerializeField] private Transform edgeCheck;   // Point placé devant l'ennemi pour détecter le vide

    private Rigidbody2D rb;
    private Animator anim;
    private Health health;

    private float stateTimer; // Chronomètre interne pour les différents états
    private int facingDirection = -1; // 1 = regarde à droite, -1 = regarde à gauche

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        health = GetComponent<Health>(); 

        currentPatrolTarget = pointB; // On commence par aller vers le point B
        UpdateFacingDirection(currentPatrolTarget.position);

        // NEW : On s'abonne à l'événement de dégâts pour jouer l'animation
        if (health != null)
        {
            health.OnDamageTaken += PlayDamageAnimation;
        }
    }

    // NEW : Se désabonner quand l'ennemi meurt pour éviter les fuites de mémoire
    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamageTaken -= PlayDamageAnimation;
        }
    }

    // NEW : Fonction appelée automatiquement par Health.cs
    private void PlayDamageAnimation()
    {
        if (anim != null) anim.SetTrigger("Damage");
    }

    void Update()
    {
        // Si l'ennemi est mort (plus de vie), on arrête tout
        if (health != null && health.CurrentHealth <= 0) return;

        // --- MACHINE À ÉTATS ---
        switch (currentState)
        {
            case State.Patrol:
                PatrolBehavior();
                CheckForPlayer();
                break;

            case State.Warning:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    // Fin du temps de chauffe, on lance la charge !
                    currentState = State.Charging;
                    stateTimer = chargeMaxDuration; // On règle le chrono pour la fin de course
                }
                break;

            case State.Charging:
                ChargeBehavior();
                break;

            case State.Recovering:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    // Fin de la récupération, on repart en patrouille
                    currentState = State.Patrol;
                    UpdateFacingDirection(currentPatrolTarget.position);
                }
                break;
        }
        //Mise à jour de l'Animator en temps réel
        UpdateAnimations();
    }

    // NEW : On synchronise les booléens avec l'état actuel
    private void UpdateAnimations()
    {
        if (anim == null) return;

        // Il marche seulement s'il est en patrouille
        anim.SetBool("isWalking", currentState == State.Patrol);

        // Il attaque seulement s'il est en train de charger
        anim.SetBool("isCharging", currentState == State.Charging);

        // NOTE : Warning et Recovering mettront automatiquement ces deux booléens sur 'false',
        // l'Animator passera donc naturellement sur "Onion Idle" grâce à tes transitions !
    }

    // ----- COMPORTEMENTS -----

    private void PatrolBehavior()
    {
        // Déplacement vers la cible
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentPatrolTarget.position.x, transform.position.y), patrolSpeed * Time.deltaTime);

        // Si on a atteint le point (à peu près), on change de cible
        if (Mathf.Abs(transform.position.x - currentPatrolTarget.position.x) < 0.2f)
        {
            currentPatrolTarget = (currentPatrolTarget == pointA) ? pointB : pointA;
            UpdateFacingDirection(currentPatrolTarget.position);
        }
    }

    private void ChargeBehavior()
    {
        // 1. Déplacement très rapide
        rb.velocity = new Vector2(chargeSpeed * facingDirection, rb.velocity.y);

        // 2. Vérifier si on a raté le joueur (fin du timer de charge)
        stateTimer -= Time.deltaTime;

        // 3. Vérifier s'il y a un mur ou un trou devant
        bool isNearEdge = !Physics2D.Raycast(edgeCheck.position, Vector2.down, 1f, groundLayer);
        // Optionnel : tu peux aussi ajouter un raycast vers l'avant pour détecter les murs

        if (stateTimer <= 0 || isNearEdge)
        {
            // On s'arrête brutalement
            rb.velocity = new Vector2(0, rb.velocity.y);
            currentState = State.Recovering;
            stateTimer = recoverDuration;
        }
    }

    // ----- DÉTECTION ET UTILITAIRES -----

    private void CheckForPlayer()
    {
        // On utilise BoxCast au lieu de Raycast pour avoir une zone de vision épaisse
        // Paramètres : position de départ, taille de la boîte, angle de rotation (0), direction, distance max, filtre de layer
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, visionBoxSize, 0f, Vector2.right * facingDirection, visionDistance, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // Le joueur est repéré ! On s'arrête.
            currentState = State.Warning;
            stateTimer = warningDuration;
            rb.velocity = Vector2.zero; // Freinage d'urgence
        }
    }

    private void UpdateFacingDirection(Vector3 targetPos)
    {
        if (targetPos.x > transform.position.x)
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

    // Dégâts de contact
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Optionnel : S'il touche le joueur pendant la charge, il peut s'arrêter
            if (currentState == State.Charging)
            {
                rb.velocity = Vector2.zero;
                currentState = State.Recovering;
                stateTimer = recoverDuration;
            }
        }
    }

    // Affiche la vision et le vide dans l'éditeur pour t'aider à configurer
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Calcule le centre mathématique du rectangle de vision pour le dessiner correctement
        Vector2 boxCenter = (Vector2)transform.position + (Vector2.right * facingDirection * (visionDistance / 2f));
        Vector2 fullBoxSize = new Vector2(visionDistance, visionBoxSize.y); // La longueur totale + la hauteur

        Gizmos.DrawWireCube(boxCenter, fullBoxSize);

        if (edgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(edgeCheck.position, Vector2.down * 1f);
        }
    }
}

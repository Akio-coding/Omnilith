using System.Collections;
using UnityEngine;

public class Gargoyle : MonoBehaviour
{
    // Les 4 états possibles de notre gargouille
    private enum State { Idle, Diving, Follow, Returning}
    private State currentState = State.Idle;

    [Header("Stats")]
    [SerializeField] private int damage = 1;

    [Header("Mouvements (Flying)")]
    [SerializeField] private float diveSpeed = 15f; // Vitesse fulgurante du plongeon
    [SerializeField] private float flySpeed = 2f; // Vitesse lente de poursuite

    [Header("Detection")]
    [SerializeField] private float visionRadius = 7f; // Zone où la gargouille repère le joueur
    [SerializeField] private float loseInterestRadius = 12f; // Zone où elle abandonne (doit être > visionRadius)

    [Header("Delay")]
    [SerializeField] private float divingDelay = 2f;

    [Header("Animations")]
    private Transform playerTransform;
    private Vector3 initialPosition; // Le perchoir d'origine
    private Vector3 diveTarget; // Le point visé lors du plongeon
    private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // On s'assure que la gravité ne l'affecte pas puisqu'elle vole
        rb.gravityScale = 0f;

        initialPosition = transform.position;

        // On cherche le joueur automatiquement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if(playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // State Machine
        switch (currentState) 
        {
            case State.Idle:
                // Si le joueur entre dans la zone de vision
                if(distanceToPlayer <= visionRadius)
                {
                    // On enregistre la position actuelle du joueur pour plonger vers ce point
                    diveTarget = playerTransform.position;

                    // On ajoute un cooldown avant le plongeon
                    StartCoroutine(DelayDive(divingDelay));
                }
                break;

            case State.Diving:
                // Lance l'animation de dive
                anim.SetBool("isDiving", true);

                // Plongeon rapide vers la cible
                transform.position = Vector3.MoveTowards(transform.position, diveTarget, diveSpeed * Time.deltaTime);

                // Si la gargouille est arrivée au point d'impact
                if(Vector3.Distance(transform.position, diveTarget) < 0.1f)
                {
                    currentState = State.Follow;
                }
                break;

            case State.Follow:
                // Retour à l'animation de vol
                anim.SetBool("isDiving", false);

                // Poursuite lente vers le joueur (qui bouge)
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);

                // Si le joueur s'est échappé assez loin
                if(distanceToPlayer > loseInterestRadius)
                {
                    currentState = State.Returning;
                }
                break;

            case State.Returning:
                // Retour lent au perchoir
                transform.position = Vector3.MoveTowards(transform.position, initialPosition, flySpeed * Time.deltaTime);

                // Si la gargouille est revenue à sa position initiale
                if(Vector3.Distance(transform.position, initialPosition) < 0.1f)
                {
                    currentState = State.Idle;
                }
                break;
        }

        FlipSprite();
    }

    private IEnumerator DelayDive(float divingDelay)
    {
        yield return new WaitForSeconds(divingDelay);
        currentState = State.Diving;
    }

    // Gestion des dégâts de contact
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform);
            }    
        }
    }

    // Gère la direction du regard du sprite
    private void FlipSprite()
    {
        Vector3 targetPos = transform.position;

        // On regarde vers la destination active
        if (currentState == State.Diving)
        {
            targetPos = diveTarget;
        }

        else if (currentState == State.Follow)
        {
            targetPos = playerTransform.position;
        }

        else if (currentState == State.Returning)
        {
            targetPos = initialPosition;
        }

        if (currentState != State.Idle)
        {
            if(targetPos.x > transform.position.x)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),transform.localScale.y,transform.localScale.z);
            }
            else if (targetPos.x < transform.position.x)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    // Pratique : Affiche les cercles de vision dans l'éditeur Unity pour t'aider à placer tes ennemis
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius); // Zone d'attaque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (transform.position, loseInterestRadius); // Zone de fuite
    }
}

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FollowingMonkey : MonoBehaviour
{
    [Header("Réglage du suivi")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followDelay = 0.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stopDistance = 1f;

    [Header("Sécurité Téléportation")]
    [SerializeField] private float maxDistanceBeforeTP = 10f; // Si > 10 unités de distances, on tp
    [SerializeField] private float tpDelay = 0.2f; // Petit délai pour l'effet visuel

    [Header("Réglages de saut")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private struct PlayerHistoryItem
    {
        public Vector3 position;
        public float timeStamp;
        public bool hasJumped;
        public PlayerHistoryItem(Vector3 pos, float time, bool jump)
        {
            position = pos;
            timeStamp = time;
            hasJumped = jump;
        }
    }

    private Queue<PlayerHistoryItem> playerHistory = new Queue<PlayerHistoryItem>();
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    [SerializeField] private bool isGrounded;
    private bool playerWasGrounded;
    private bool isTeleporting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (isTeleporting)
        {
            return; // On stoppe la logique si on est en train de se TP
        }

        RecordHistory();
        CheckGroundStatus();
        CheckForTeleport();
        MoveMonkey();
    }

    private void CheckForTeleport()
    {
        float currentDistance = Vector2.Distance(transform.position, playerTransform.position);

        // Si le singe est trop loin du joueur
        if (currentDistance > maxDistanceBeforeTP)
        {
            StartCoroutine(TeleportToPlayer());
        }
    }

    private System.Collections.IEnumerator TeleportToPlayer()
    {
        isTeleporting = true;

        // Optionnel : Petit effet de disparition (on peut aussi déclencher une animation ici)
        if (sr != null)
        {
            sr.enabled = false;
        }
        // On vide l'historique pour ne pas qu'il essaie de refaire le chemin en arrivant
        playerHistory.Clear();
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(tpDelay);

        // On le place un peu derrière le joueur
        transform.position = playerTransform.position;
        if (sr != null)
        {
            sr.enabled = true;
        }
        isTeleporting = false;
    }

    private void RecordHistory()
    {
        bool playerJumpedThisFrame = false;
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if(playerRb != null)
        {
            if (playerRb.velocity.y > 1f && playerWasGrounded)
            {
                playerJumpedThisFrame = true;
                playerWasGrounded = false;
            }

            else if (Mathf.Abs(playerRb.velocity.y) < 0.1f)
            {
                playerWasGrounded = true;
            }

            playerHistory.Enqueue(new PlayerHistoryItem(playerTransform.position, Time.time, playerJumpedThisFrame));
        }
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void MoveMonkey()
    {
        // On ne fait rien si la liste est vide ou si le temps n'est pas venu
        if (playerHistory.Count == 0 || Time.time < playerHistory.Peek().timeStamp + followDelay)
        {
            StopMoving();
            return;
        }

        PlayerHistoryItem targetStep = playerHistory.Peek();
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Gestion du saut
        if (targetStep.hasJumped && isGrounded)
        {
            Jump();
        }

        // 2. Gestion du déplacement horizontal
        if (distanceToPlayer > stopDistance)
        {
            float direction = targetStep.position.x > transform.position.x ? 1 : -1;
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

            if (animator != null) animator.SetBool("isWalking", true);
            if (sr != null) sr.flipX = direction < 0;
        }
        else
        {
            StopMoving();
            // Si on est déjà sur le joueur, on vide tout pour éviter de bégayer
            if (distanceToPlayer < stopDistance * 0.5f) playerHistory.Clear();
        }

        // 3. IMPORTANT : On passe au point suivant si on est arrivé à la position X du point
        // On met cette ligne EN DEHORS du "if distanceToPlayer"
        if (playerHistory.Count > 0 && Mathf.Abs(transform.position.x - targetStep.position.x) < 0.2f)
        {
            playerHistory.Dequeue();
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        if (animator != null)
        {
            animator.SetTrigger("jump");
        }
    }

    private void StopMoving()
    {
        rb.velocity = new Vector2(0,rb.velocity.y);
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }
}
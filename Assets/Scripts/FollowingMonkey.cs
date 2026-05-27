using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class FollowingMonkey : MonoBehaviour
{
    [Header("Réglage du suivi")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followDelay = 0.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stopDistance = 1f;

    [Header("Réglage du suivi (Avec Objet)")]
    [SerializeField] private float carryFollowDelay = 0.1f;
    [SerializeField] private float carryStopDistance = 0.5f;

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

    [Header("Système de Lancer")]
    [SerializeField] private float detectionRadius = 2f; // Distance pour ramasser
    [SerializeField] private LayerMask throwableLayer; // Layer des objets lancables
    [SerializeField] private Transform throwPoint; // Un point vide devant le singe
    [SerializeField] private Vector2 highThrowForce = new Vector2(5f, 10f); // Force vers le haut
    [SerializeField] private Vector2 lowThrowForce = new Vector2(8f, 3f);   // Force ras du sol

    private GameObject carriedObject;

    [Header("Système de lancer de joueur")]
    [SerializeField] private Vector2 playerThrowForce = new Vector2(4f, 8f);
    private bool isChasingPlayer = false; // Indique si le singe essaye d'attraper le joueur 

    // Unlocking the monkey
    private bool isUnlocked = false;

    // -------------------------------------

    [SerializeField] private bool isGrounded;
    private Queue<PlayerHistoryItem> playerHistory = new Queue<PlayerHistoryItem>();
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private bool playerWasGrounded;
    private bool isTeleporting = false;
    private Vector3 lastRecordedPos;

    private void OnEnable()
    {
        DialogueManager.OnMonkeyUnlocked += UnlockMonkey;
    }

    private void OnDisable()
    {
        DialogueManager.OnMonkeyUnlocked -= UnlockMonkey;
    }

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
        // Si le singe n'est pas débloqué 
        if (!isUnlocked)
        {
            CheckGroundStatus();
            StopMoving();
            return;
        }

        if (isTeleporting)
        {
            return; // On stoppe la logique si on est en train de se TP
        }

        HandleInteraction();

        if (isChasingPlayer) 
        {
            ChasePlayerForPickUp();
        }

        // Si l'objet porté est le joueur, le singe s'arrête d'essayer de le suivre !
        else if (carriedObject == playerTransform.gameObject)
        {
            StopMoving();
            CheckGroundStatus();
            // On ne lit ni RecordHistory() ni MoveMonkey(), il attend sagement tes ordres.
        }

        else
        {
            RecordHistory();
            CheckGroundStatus();
            CheckForTeleport();
            MoveMonkey();
        }
            
        if (carriedObject != null && throwPoint != null)
        {
            carriedObject.transform.position = throwPoint.position;
            // On force la vitesse du joueur à 0 pour éviter qu'il tremble en essayant de bouger
            carriedObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
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
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        bool playerJumpedThisFrame = false;

        // Détection du saut
        if (playerRb.velocity.y > 1f && playerWasGrounded)
        {
            playerJumpedThisFrame = true;
            playerWasGrounded = false;
        }
        else if (Mathf.Abs(playerRb.velocity.y) < 0.1f)
        {
            playerWasGrounded = true;
        }

        // NOUVEAU : On enregistre seulement si on a bougé de 0.1 unité OU si on a sauté
        float distanceSinceLastPoint = Vector3.Distance(playerTransform.position, lastRecordedPos);

        if (distanceSinceLastPoint > 0.1f || playerJumpedThisFrame)
        {
            playerHistory.Enqueue(new PlayerHistoryItem(playerTransform.position, Time.time, playerJumpedThisFrame));
            lastRecordedPos = playerTransform.position;
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

        float currentDelay = (carriedObject != null) ? carryFollowDelay : followDelay;
        float currentStop = (carriedObject != null) ? carryStopDistance : stopDistance;

        // On ne fait rien si la liste est vide ou si le temps n'est pas venu
        if (playerHistory.Count == 0 || Time.time < playerHistory.Peek().timeStamp + currentDelay)
        {
            StopMoving();
            return;
        }

        PlayerHistoryItem targetStep = playerHistory.Peek();
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. GESTION DU SAUT (Sécurisée et Intelligente)
        // Le singe saute si on lui dit de sauter, OU si le point cible est au-dessus de lui
        bool targetIsAbove = targetStep.position.y > transform.position.y + 0.8f;
        bool needsToJump = targetStep.hasJumped || targetIsAbove;

        if (needsToJump && isGrounded)
        {
            Jump();
        }

        // 2. GESTION DU DÉPLACEMENT HORIZONTAL
        if (distanceToPlayer > currentStop && carriedObject != playerTransform.gameObject)
        {
            float direction = targetStep.position.x > transform.position.x ? 1 : -1;
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

            if (animator != null) animator.SetBool("isWalking", true);

            Vector3 localScale = transform.localScale;
            // On garde la taille d'origine (Mathf.Abs) mais on inverse le signe
            localScale.x = direction < 0 ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
        else
        {
            StopMoving();
        }

        // 3. PASSAGE AU POINT SUIVANT
        float distanceToPointX = Mathf.Abs(transform.position.x - targetStep.position.x);
        bool pointTooOld = Time.time > targetStep.timeStamp + followDelay + 2.0f;

        if (distanceToPointX < 0.2f || pointTooOld)
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

    private void HandleInteraction()
    {
        // Si on appuie sur 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (carriedObject == null)
            {
                if (isChasingPlayer)
                {
                    isChasingPlayer = false; // Annule la course vers le joueur si on a appuyé par erreur
                }
                else
                {
                    TryPickUp();
                }
            }
            else
            {
                ThrowObject();
            }
        }

        // Libérer le joueur du singe
        // On verifie si il porte un objet ET que cet objet est le joueur 
        if (carriedObject != null && carriedObject == playerTransform.gameObject) 
        {
            if (Input.GetButtonDown("Fire1")) 
            {
                DropObject();
            }
        }
    }

    private void TryPickUp()
    {
        // Détecte les objets autour du singe
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, throwableLayer);

        if (hit != null)
        {
            // On lance l'animation
            animator.SetTrigger("Punch");

            carriedObject = hit.gameObject;
            // On désactive la physique de l'objet pendant qu'on le porte
            carriedObject.GetComponent<Rigidbody2D>().isKinematic = true;
            carriedObject.GetComponentInChildren<Collider2D>().enabled = false;

            Crab crabScript = carriedObject.GetComponentInParent<Crab>();
            if (crabScript != null)
            {
                crabScript.PickUpByMonkey();
            }
        }
        else
        {
            // Pas d'objet ? ==> on part attraper le joueur
            isChasingPlayer = true;
            playerHistory.Clear();
        }
    }

    private void ChasePlayerForPickUp()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= stopDistance * detectionRadius)
        {
            // On lance l'animation
            animator.SetTrigger("Punch");

            carriedObject = playerTransform.gameObject;

            // On désactive la physique du joueur pour éviter les déplacements 
            Rigidbody2D prb = carriedObject.GetComponent<Rigidbody2D>();
            prb.isKinematic = true;
            prb.velocity = Vector2.zero;
            carriedObject.GetComponentInChildren<Collider2D>().enabled = false;

            isChasingPlayer = false;
            StopMoving();
        }
        else
        {
            // mouvement vers le joueur 
            float direction = playerTransform.position.x > transform.position.x ? 1 : -1;
            rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }

            Vector3 localScale = transform.localScale;
            localScale.x = direction < 0 ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        }
    }

    private void ThrowObject()
    {
        // On lance l'animation
        animator.SetTrigger("Punch");

        // On détache l'objet
        Rigidbody2D objRb = carriedObject.GetComponent<Rigidbody2D>();
        carriedObject.GetComponentInChildren<Collider2D>().enabled = true;
        objRb.isKinematic = false;

        // Détermine la direction (basée sur le flipX du sprite du singe)
        float lookDir = transform.localScale.x < 0 ? -1f : 1f;
        Vector2 finalForce;

        // Si l'objet porté est le joueur
        if (carriedObject == playerTransform.gameObject)
        {
            finalForce = new Vector2(playerThrowForce.x * lookDir, playerThrowForce.y);
        }

        else
        {
            // Type de lancer : Vers le haut si on maintient 'Z' ou 'UpArrow'
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                finalForce = new Vector2(highThrowForce.x * lookDir, highThrowForce.y);
            }
            else // Lancer ras du sol par défaut
            {
                finalForce = new Vector2(lowThrowForce.x * lookDir, lowThrowForce.y);
            }
        }
        
        carriedObject.transform.rotation = Quaternion.identity;

        objRb.AddForce(finalForce, ForceMode2D.Impulse);

        Crab crabScript = carriedObject.GetComponentInParent<Crab>();
        if (crabScript != null)
        {
            crabScript.ThrowByMonkey();
        }

        carriedObject = null;
    }

    private void DropObject()
    {
        if (carriedObject == null)
        {
            return;
        }

        // Remttre l'objet/joueur a la normale
        Rigidbody2D objRb = carriedObject.GetComponent<Rigidbody2D>();
        carriedObject.GetComponentInChildren<Collider2D>().enabled = true;
        objRb.isKinematic = false;

        // Remettre la chose portée droite 
        carriedObject.transform.rotation =Quaternion.identity;

        // Reset vitesse 
        objRb.velocity = Vector2.zero;

        Crab crabScript = carriedObject.GetComponentInParent<Crab>();
        if (crabScript != null)
        {
            crabScript.DropByMonkey();
        }

        carriedObject = null;
    }

    private void UnlockMonkey()
    {
        isUnlocked = true;
        playerHistory.Clear();

        if(playerTransform != null)
        {
            lastRecordedPos = playerTransform.position;
        }

        Debug.Log("le singe rejoins l'aventure");
    }

}
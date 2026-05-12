using UnityEngine;

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
        if(anim != null)
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

        transform.position = Vector2.MoveTowards(transform.position, new Vector2(currentPatrolTargert.position.x, transform.position.y), patrolSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - currentPatrolTargert.position.x) < 0.1f)
        {
            StartPause();
        }
    }

    private void StartPause()
    {
        currentState = State.Pause;
        pauseTimer = pauseDuration;
        rb.velocity = Vector2.zero;
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
            rb.velocity = Vector2.zero;
            currentState = State.Pause;
            pauseTimer = 1f;
            return;
        }

        UpdateFacingDirection(playerTransform.position);

        Vector2 targetPos = new Vector2(playerTransform.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);

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
        rb.velocity = Vector2.zero;

        if(damageDealer != null)
        {
            damageDealer.enabled = false;
        }
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

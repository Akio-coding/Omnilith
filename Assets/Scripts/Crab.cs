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
        }
    }
}

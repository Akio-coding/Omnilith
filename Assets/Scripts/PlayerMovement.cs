using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Réglages du Saut")]
    [Tooltip("La force initiale du saut.")]
    [SerializeField] private float jumpForce = 15f;

    [Tooltip("À quel point le saut est coupé si on relâche le bouton (0 à 1). 0.5 = coupe la vitesse de moitié.")]
    [Range(0, 1)] [SerializeField] private float shutJump = 0.5f;

    [Tooltip("Gravité augmentée en retombant.")]
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Vérification Sol")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Player Mouvements settings")]
    [SerializeField] private float speed = 4;

    // --- Dash ---
    [Header("Réglages du Dash")]
    [SerializeField] private float dashDistance = 5f;      
    [SerializeField] private float dashDuration = 0.4f;  
    [SerializeField] private float dashCooldown = 1f;

    [Header("Collision Dash")]
    [SerializeField] private BoxCollider2D collider;
    [Tooltip("La taille du collider pendant le dash (x, y)")]
    [SerializeField] private Vector2 dashColliderSize;
    [Tooltip("Le décalage du centre pour que les pieds restent au sol")]
    [SerializeField] private Vector2 dashColliderOffset;

    // Variables
    private bool isDashing;
    private bool canDash = true;
    private float actualTime;
    private float previousCurveValue;
    private TrailRenderer tr;
    
    // --- Collider ---
    private Vector2 initialSize;
    private Vector2 initialOffset;
    
    
    // --- End Dash ---

    private bool isOnGround;
    private float initialGravity;

    private int facingDirection = 1; // 1 = right -1 = left

    public AnimationCurve dashSpeedCurve;
    private Rigidbody2D rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();

        initialGravity = rb.gravityScale;
        initialSize = collider.size;
        initialOffset = collider.offset;
    }

    // Update is called once per frame
    void Update()
    {
        VerifyIfOnGround(); 
        InputManager();

        if (isDashing)
        {
            Dash();
        }
        else if (!isDashing)
        {
            GravityHandler();
            Movements(); 
        }
    }
    void VerifyIfOnGround()
    {
        isOnGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    void InputManager()
    {
        if (Input.GetButtonDown("Jump") && isOnGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Check when we release the jump button to stop the jump movement
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * shutJump);
        }

        // DASH (Left Shift Key by default "Fire3" or check input manager
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartDash();            
        }
    }

    // Sets all the variables needed to dash 
    void StartDash()
    {
        canDash = false;
        isDashing = true;
        actualTime = 0f;
        previousCurveValue = 0f;

        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        // change collider size
        collider.size = dashColliderSize;
        collider.offset = dashColliderOffset;
    }
    
    void Dash()
    {
        actualTime += Time.deltaTime;
        float dashPercent = actualTime / dashDuration;

        if (dashPercent >= 1f)
        {
            EndDash();
            return;
        }

        // We calculate the delta between actual curve value and the previous one,
        // then we calculate in percent the distance to move
        // then we move forward a distance corresponding to the value

        float currentCurveValue = dashSpeedCurve.Evaluate(dashPercent);

        float deltaDistance = currentCurveValue - previousCurveValue;

        float distanceThisFrame = dashDistance * deltaDistance;

        // Convert distance into velocity
        float velocityThisFrame = distanceThisFrame / Time.deltaTime;
        rb.velocityX = facingDirection * velocityThisFrame;

        previousCurveValue = currentCurveValue;
    }

    // Resets all the variables 
    void EndDash()
    {
        isDashing = false;
        rb.gravityScale = initialGravity;

        rb.velocity = new Vector2(speed,0);

        // return to initial collider
        collider.size = initialSize;
        collider.offset = initialOffset;


        // Wait for cooldown 
        StartCoroutine(DashCooldownRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void GravityHandler()
    {
        // intensify gravity when falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = initialGravity * fallMultiplier;
        }
        else
        {
            rb.gravityScale = initialGravity;
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    void Movements()
    {
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            facingDirection = 1;
        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            facingDirection = -1;
        }

        rb.velocity = new Vector2(inputX * speed, rb.velocity.y);
    }

}


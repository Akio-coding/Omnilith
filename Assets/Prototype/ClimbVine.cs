using UnityEngine;

public class ClimbVine : MonoBehaviour
{
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool isTouchingVine;
    [SerializeField] private bool isClimbing;
    [SerializeField] private float verticalInput;
    [SerializeField] private float defaultGravity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; //permet de sauvegarder la velocité 

    }

    // Update is called once per frame
    void Update()
    {
        verticalInput = Input.GetAxisRaw("Vertical");

        if (isTouchingVine && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }
    }
    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, verticalInput * climbSpeed);

        }
        else
        {
            rb.gravityScale = defaultGravity;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Vines"))
        {
            isTouchingVine = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Vines"))
        {
            isTouchingVine = false;
            isClimbing = false; 
        }
    }
}

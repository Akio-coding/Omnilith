using UnityEngine;

public class Slime : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 2f;

    [Header("Détection")]
    [SerializeField] private Transform groundCheck; // Le point devant et en bas
    [SerializeField] private Transform wallCheck;   // Le point droit devant
    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer; // Le calque de tes plateformes

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // TRÈS IMPORTANT : On désactive la gravité physique pour ce monstre
        rb.gravityScale = 0f;
        rb.isKinematic = true; // C'est notre script qui force le mouvement, pas le moteur physique
    }

    void Update()
    {
        // 1. Le slime avance toujours "tout droit" par rapport à lui-même
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 2. On lance l'antenne vers le bas (pour vérifier s'il a encore du sol sous lui)
        bool isGrounded = Physics2D.Raycast(groundCheck.position, -transform.up, checkDistance, groundLayer);

        // 3. On lance l'antenne vers l'avant (pour voir s'il fonce dans un mur)
        bool hittingWall = Physics2D.Raycast(wallCheck.position, transform.right, checkDistance, groundLayer);

        // 4. Logique de rotation
        if (hittingWall)
        {
            // Il y a un mur devant : on pivote de 90 degrés (il se cabre pour grimper)
            transform.Rotate(0, 0, -90f);
        }
        else if (!isGrounded)
        {
            // Il n'y a plus de sol : il est au bord d'un trou ! On pivote vers le bas pour faire le tour.
            transform.Rotate(0, 0, 90f);

            // On le pousse très légèrement en avant pour qu'il "colle" à la nouvelle surface
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        }
    }

    // Affiche les antennes dans l'éditeur pour t'aider à les placer
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(groundCheck.position, -transform.up * checkDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(wallCheck.position, transform.right * checkDistance);
        }
    }
}

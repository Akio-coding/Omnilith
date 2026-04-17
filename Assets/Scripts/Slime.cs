using UnityEngine;
using System.Collections;

public class Slime : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 300f; // La vitesse de la rotation (en degrés par seconde)
    
    [Header("Détection")]
    [SerializeField] private Transform groundCheck; // Le point devant et en bas
    [SerializeField] private Transform wallCheck;   // Le point droit devant
    [SerializeField] private float checkDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer; // Le calque de tes plateformes

    private Rigidbody2D rb;
    private bool isRotating = false; // NOUVEAU : Un verrou pour bloquer les actions pendant qu'il tourne

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // TRÈS IMPORTANT : On désactive la gravité physique pour ce monstre
        rb.gravityScale = 0f;
        rb.isKinematic = true; // C'est notre script qui force le mouvement, pas le moteur physique
    }

    void Update()
    {
        // NOUVEAU : Si le slime est en pleine animation de rotation, on empêche l'Update de le faire avancer
        if (isRotating) return;

        // 1. Le slime avance toujours "tout droit" par rapport à lui-même
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 2. On lance l'antenne vers le bas (pour vérifier s'il a encore du sol sous lui)
        bool isGrounded = Physics2D.Raycast(groundCheck.position, -transform.up, checkDistance, groundLayer);

        // 3. On lance l'antenne vers l'avant (pour voir s'il fonce dans un mur)
        bool hittingWall = Physics2D.Raycast(wallCheck.position, transform.right, checkDistance, groundLayer);

        // 4. Logique de rotation
        if (hittingWall)
        {
            // On lance la rotation fluide au lieu de la téléportation d'angle
            StartCoroutine(SmoothRotation(-90f, false));
        }
        else if (!isGrounded)
        {
            // Pareil pour le vide
            StartCoroutine(SmoothRotation(90f, true));
        }
    }

    // La fonction qui gère l'animation de rotation image par image
    private IEnumerator SmoothRotation(float angleOffset, bool isEdge)
    {
        // On active le verrou pour bloquer le mouvement dans Update()
        isRotating = true;

        // On calcule la rotation mathématique cible
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, angleOffset);

        // Tant qu'on n'a pas (presque) atteint l'angle cible...
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            // On tourne progressivement vers la cible
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // On attend la frame suivante avant de continuer la boucle
            yield return null;
        }

        // On force la rotation finale exacte pour éviter les micro-erreurs de calcul (ex: 89.999°)
        transform.rotation = targetRotation;

        // Si c'est un précipice, on applique la petite correction pour "coller" au mur
        if (isEdge)
        {
            // On le pousse un peu en avant (X) et un peu vers le haut (Y) pour le sortir du mur.
            // Si 0.1f ne suffit pas, essaie 0.15f ou 0.2f !
            transform.Translate(new Vector2(0.1f, 0.1f));
        }
        // On retire le verrou, le slime va recommencer à avancer à la prochaine frame !
        isRotating = false;
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

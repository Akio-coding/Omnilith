using UnityEngine;

public class ProjectileTrigger : MonoBehaviour
{
    [Header("Cible")]
    public GameObject objectToDestroy; // Le mur à faire disparaître

    [Header("Réglages")]
    public string projectileTag = "Fireball"; // Tag de ta boule de feu
    public bool destroyProjectile = true;     // Est-ce que la boule de feu disparaît en touchant ?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. On vérifie si c'est le Joueur (via son parent comme avant)
        Rigidbody2D playerRb = collision.GetComponentInParent<Rigidbody2D>();
        bool isPlayer = playerRb != null && playerRb.CompareTag("Player");

        // 2. On vérifie si c'est la boule de feu (via son Tag)
        bool isProjectile = collision.CompareTag(projectileTag);

        if (isPlayer || isProjectile)
        {
            OpenAccess();

            // Si c'est la boule de feu qui a touché, on la détruit
            if (isProjectile && destroyProjectile)
            {
                Destroy(collision.gameObject);
            }
        }
    }

    private void OpenAccess()
    {
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
            Debug.Log("Accès ouvert par projectile ou contact !");
        }
    }
}

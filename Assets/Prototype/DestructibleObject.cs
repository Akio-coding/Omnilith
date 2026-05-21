using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Réglages")]
    public string projectileLayerName = "Player Projectile";
    public bool destroyProjectileToo = true; // Est-ce que la boule de feu disparaît aussi ?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie si l'objet qui nous touche est sur le layer des projectiles
        if (collision.gameObject.layer == LayerMask.NameToLayer(projectileLayerName))
        {
            // 1. (Optionnel) On détruit le projectile qui nous a touché
            if (destroyProjectileToo)
            {
                Destroy(collision.gameObject);
            }

            // 2. On déclenche la destruction
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        Debug.Log(gameObject.name + " a été détruit par un projectile !");

        // On détruit l'objet sur lequel est posé ce script
        Destroy(gameObject);
    }
}

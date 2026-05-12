using UnityEngine;

public class LayerTrigger : MonoBehaviour
{
    [Header("Cible")]
    public GameObject objectToDestroy; // Le mur ou l'accès à ouvrir

    [Header("Réglages Physiques")]
    public string projectileLayerName = "Player Projectile";
    public bool destroyProjectileOnImpact = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Détection du Joueur (on remonte au parent qui a le Rigidbody)
        Rigidbody2D playerRb = collision.GetComponentInParent<Rigidbody2D>();
        bool isPlayer = playerRb != null && playerRb.CompareTag("Player");

        // 2. Détection par Layer (Boule de feu)
        // On compare le layer de l'objet qui touche avec celui configuré
        bool isProjectile = collision.gameObject.layer == LayerMask.NameToLayer(projectileLayerName);

        if (isPlayer || isProjectile)
        {
            OpenAccess();

            // Si c'est un projectile et qu'on veut le détruire à l'impact
            if (isProjectile && destroyProjectileOnImpact)
            {
                Destroy(collision.gameObject);
            }
        }
    }

    private void OpenAccess()
    {
        if (objectToDestroy != null)
        {
            // Ici tu peux mettre un Destroy ou un SetActive(false)
            Destroy(objectToDestroy);
            Debug.Log("Accès ouvert ! Déclenché par : " + projectileLayerName);
        }
    }
}
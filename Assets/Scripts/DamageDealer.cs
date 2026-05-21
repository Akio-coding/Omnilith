using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    // ---- Variables ----
    [SerializeField] private int damageAmount = 1;

    [Header("Rebond (Attaque vers le bas)")]
    [Tooltip("Coche cette case pour que cette attaque fasse rebondir le joueur !")]
    [SerializeField] private bool bounceOnHit = false;
    [Tooltip("La force du rebond vers le haut")]
    [SerializeField] private float bounceForce = 15f;
    // ---- Functions ----

    // We use OnTriggerEnter2D because Hurtbox is a Trigger 
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // If the touched object is on the PlayerHitbox layer we stop the function
        // Prevent the player being hurt because of his sword
        if (collider.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox"))
        {
            return;
        }

        // Search the Health script in the object if it don t find it, it will search in the parents
        Health targetHealth = collider.GetComponentInParent<Health>();

        // If the target has an Health component 
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageAmount, transform);
            Debug.Log("you took damages");
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TriggerBounce();
        }
    }

    // OnCollisionEnter2D is for physical contact (we can't pass through the object) (for non-trigger objects)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Same as above 
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox"))
        {
            return;
        }

        // Same as above, this will search the Health script 
        Health targetHealth = collision.gameObject.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageAmount, transform);
            Debug.Log("damages collision");
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            TriggerBounce();
        }  
    }

    private void TriggerBounce()
    {
        if (bounceOnHit)
        {
            // On cherche le Rigidbody du joueur (qui est le parent de l'épée)
            Rigidbody2D playerRb = GetComponentInParent<Rigidbody2D>();
            if(playerRb != null)
            {
                // On force la vitesse Y du joueur pour garantir un rebond constant, 
                // même s'il était en train de tomber très vite !
                playerRb.velocity = new Vector2 (playerRb.velocity.x, bounceForce); 
            }
        }
    }
}

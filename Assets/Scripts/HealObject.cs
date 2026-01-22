using UnityEngine;

public class HealObject : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;

    // We use OnTriggerEnter2D because Hurtbox is a Trigger 
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // we need to search the Health component on the touched object
        // If we touch the hurtbox, Health script is in parent 
        Health targetHealth = collider.GetComponentInParent<Health>();

        // If the target has an Health component 
        if (targetHealth != null)
        {
            targetHealth.Heal(healAmount);
            Destroy(gameObject);
        }
    }

    // OnCollisionEnter2D is for physical contact (we can't pass through the object) (for non-trigger objects)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Doesn't need to search for parent because the player collider is directly on him
        Health targetHealth = collision.gameObject.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            targetHealth.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}

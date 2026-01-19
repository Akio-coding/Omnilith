using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 2f; 
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    // Cette fonction sera appelée par le joueur au moment du tir pour donner la direction
    public void SetDirection(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = direction * speed;
    }

    
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // ignore the player
        if (hitInfo.CompareTag("Player")) return;

        // Ici, plus tard, on mettra le code pour blesser l'ennemi :
        // Enemy enemy = hitInfo.GetComponent<Enemy>();
        // if (enemy != null) enemy.TakeDamage(damage);

        // On détruit la boule de feu (effet d'impact)
        Destroy(gameObject);
    }
}

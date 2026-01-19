using UnityEngine;

public class PlayerFireBall : MonoBehaviour
{
    [Header("Réglages Attaque")]
    [SerializeField] private Transform spawnPoint; 
    [SerializeField] private GameObject fireballPrefab; 
    [SerializeField] private float fireRate = 0.5f;


    private float nextFireTime = 0f;
    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Si on appuie sur F (ou Fire1/Clic gauche) et que le temps de recharge est passé
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Create a fireball at the spawn point
        GameObject bullet = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Calculer la direction (Droite ou Gauche) selon le Sprite du joueur
        // Note : Ton script de mouvement utilise flipX, donc on regarde ça.
        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;

        // 3. Envoyer la direction au script de la boule de feu
        //bullet.GetComponent<Projectile>().SetDirection(direction);
    }
}

using UnityEngine;

public class PlayerFireBall : MonoBehaviour
{
    [Header("Réglages Attaque")]
    [SerializeField] private Transform spawnPoint; 
    [SerializeField] private GameObject fireballPrefab; 
    [SerializeField] private float fireRate = 0.5f;

    [SerializeField] private int manaCost = 1;

    private float nextFireTime = 0f;
    private SpriteRenderer sr;
    private ManaManager manaManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // On récupère automatiquement le script ManaManager qui est sur le joueur
        manaManager = GetComponent<ManaManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Si on appuie sur F (ou Fire1/Clic gauche) et que le temps de recharge est passé
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            // NOUVEAU : On demande au ManaManager si on a assez de charges
            if (manaManager != null && manaManager.TryConsumeCharge(manaCost))
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else if (manaManager == null)
            {
                // Si on a oublié de mettre le script ManaManager, l'attaque marche quand même par sécurité
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
            else
            {
                Debug.Log("Pas assez de charges pour lancer le sort !");
            }
        }
    }

    void Shoot()
    {
        // Create a fireball at the spawn point
        GameObject bullet = Instantiate(fireballPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. Calculer la direction (Droite ou Gauche) selon le Sprite du joueur
        // Note : Ton script de mouvement utilise flipX, donc on regarde ça.
        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;

        FireBall fireBallScript = bullet.GetComponent<FireBall>();
        if (fireBallScript != null)
        {
            fireBallScript.SetDirection(direction);
        }
    }
}

using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Réglages Attaque")]
    [SerializeField] private Collider2D attackCollider;
    [Tooltip("Combien d'attaques le joueur peut lancer par seconde")]
    [SerializeField] private float attackRate = 2f; // How many attacks per seconds 

    private float nextAttackTime = 0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Ensure that the sword collider is deactivated at the start
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetButtonDown("Fire1")) // Left clic or Ctrl
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        // Play the animation, the functions below will be called in it
        anim.SetTrigger("Attack");
    }

    // --- FUNCTIONS CALLED BY THE ANIMATOR ---
    
    // 1. At the beginning of the movement (when the sword becomes dangerous)
    public void EnableHitbox()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
    }

    // 2. At the end of the movement (when we want to stop the attack) 
    public void DisableHitbox()
    {
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }
}

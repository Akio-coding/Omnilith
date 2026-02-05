using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Colliders d'attaque")]
    [SerializeField] private Collider2D SlashCollider;
    [SerializeField] private Collider2D StabCollider;

    [Header("Gestion timing")]
    [Tooltip("Pendant combien de temps le joueur peut appuyer pour lancer la 2nd attaque du combo")]
    [SerializeField] private float attackBuffer = 0.5f; // Time during you can buffer the attack
    [Tooltip("Combien d'attaques le joueur peut lancer par seconde")]
    [SerializeField] private float attackRate = 2f; // How many attacks per seconds, Not this usefull i guess ?
    [Tooltip("Nombre de coups maximum dans un combo")]
    [SerializeField] private int maxHitInCombo = 2;

    // --- Variables ---
    // -- Combo --
    //private bool bisAttacking = false;
    private bool canCombo = false;
    private int comboCounter = 0;

    // -- General --
    private float nextAttackTime = 0f;
    private float lastAttackTime = 0f;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Ensure that the sword collider is deactivated at the start
        if (SlashCollider != null)
        {
            SlashCollider.enabled = false;
        }
        if (StabCollider != null) 
        {
            StabCollider.enabled = false;
        }
    }

    void Update()
    {
        //udate => launch a timer to see if u can do a combo
        //if timer < attackBuffer & combocounter < 3=> startcombo, combocounter = 1, if input => combocounter++, launch attack, resetBuffer

        if (comboCounter >= 1 && canCombo)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Debug.Log("attacking");
                comboCounter++;
                Attack();
                nextAttackTime = Time.time + (1f / attackRate);
                if (comboCounter >= maxHitInCombo)
                {
                    Debug.Log("resetting");
                    canCombo = false;
                    comboCounter = 0;
                }
            }
        }


        if (Time.time >= nextAttackTime)
        {
            if (Input.GetButtonDown("Fire1")) // Left clic or Ctrl
            {
                comboCounter = 0;
                comboCounter++;
                Attack();
                nextAttackTime = Time.time + ( 1f / attackRate);
                canCombo = true;
            }
        }
    }

    void Attack()
    {
        // Play the animation, the functions below will be called in it
        anim.SetTrigger("Attack");
        anim.SetInteger("ComboStep", comboCounter); // Pour savoir quelle anim jouer
    }

    // --- FUNCTIONS CALLED BY THE ANIMATOR ---
    
    // 1. At the beginning of the movement (when the sword becomes dangerous)
    public void EnableHitbox()
    {
        if (comboCounter == 3)
        {
            if(StabCollider != null)
            {
                StabCollider.enabled = true;
            }
        }
        else
        {
            if (SlashCollider != null)
            { 
                SlashCollider.enabled = true;
            }
        }
        
    }

    // 2. At the end of the movement (when we want to stop the attack) 
    public void DisableHitbox()
    {
        if (SlashCollider != null)
        {
            SlashCollider.enabled = false;
        }
        if (StabCollider!= null)
        {
            StabCollider.enabled = false;
        }
    }
}

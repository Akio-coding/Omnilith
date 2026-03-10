using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Colliders d'attaque")]
    [SerializeField] private Collider2D SlashCollider;
    [SerializeField] private Collider2D StabCollider;
    [SerializeField] private Collider2D UpAttackCollider;

    [Header("Gestion timing")]
    [Tooltip("Nombre de coups maximum dans un combo")]
    [SerializeField] private int maxHitInCombo = 3;
    [Tooltip("Timing toléré après l'animation de l'attaque qui accepte le trigger Time allowed after an attack animation to still trigger the next combo step")]
    [SerializeField] private float comboTolerance = 0.5f;

    [Header("Movement")]
    [Tooltip("Force du petit pas en avant lors de l'attaque")]
    [SerializeField] private float attackStepForce = 10f;
    
    // --- Variables ---
    // -- State --
    public bool IsAttacking { get; private set; }
    private bool isAttackingUp = false;
    private bool inputBuffered = false;
    private int comboCounter = 0;
    private float lastAttackEndTime = 0f; // New: To track the tolerance window

    // -- General --
    private Animator anim;
    private PlayerMovement movement;

    void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        // Ensure that the sword collider is deactivated at the start
        DisableHitbox();
    }

    void Update()
   {
    // 1. Check for Timeout: Reset combo if user waited too long after the last attack
        if (!IsAttacking && comboCounter > 0)
        {
            if (Time.time > lastAttackEndTime + comboTolerance)
            {
                comboCounter = 0; // Combo dropped
            }
        }

        // 2. Input Handling
        if (Input.GetButtonDown("Fire1"))
        {
            // Verify if we input in up direction
            float yInput = Input.GetAxisRaw("Vertical");

            if (IsAttacking)
            {
                // Case A: Player presses DURING animation -> Buffer the input
                if (comboCounter < maxHitInCombo)
                {
                    inputBuffered = true;
                }
            }
            else
            {
                // If we input up => UpAttack
                if(yInput > 0.1f)
                {
                    StartUpAttack();
                }
                else
                {
                    // Case B: Player presses when NOT attacking (Start fresh OR Continue combo in tolerance window)
                    StartAttack();
                }
                    
            }
        }
    }

    private void StartAttack()
    {
        // Reset logic
        inputBuffered = false;
        IsAttacking = true;
        isAttackingUp = false;

        // Loop the combo if we exceeded max hits (Optional, depends on design)
        if (comboCounter >= maxHitInCombo)
        {
            comboCounter = 0;
        }

        // Increment to next step
        comboCounter++;

        // Update Animator
        anim.SetTrigger("Attack");
        anim.SetInteger("ComboStep", comboCounter);
    }

    private void StartUpAttack()
    {
        inputBuffered = false;
        IsAttacking = true;
        isAttackingUp = true;

        // On ne touche pas au comboCounter pour l'attaque haute (souvent c'est un coup unique)
        // Ou tu peux décider qu'elle casse le combo, à toi de voir.
        anim.SetTrigger("AttackUp");
    }

    // --- FUNCTIONS CALLED BY THE ANIMATOR ---

    // 1. At the beginning of the movement (when the sword becomes dangerous)
    private void EnableHitbox()
    {
        // Selection of the hitbox
        if (isAttackingUp) 
        {
            // Attack up
            if(UpAttackCollider != null)
            {
                UpAttackCollider.enabled = true;
            }
        }
        else
        {
            // Normal case (combo attack)
            if (comboCounter >= 3)
            {
                if (StabCollider != null)
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
        
    }

    // At the end of the attack (when we want to stop the attack) 
    private void DisableHitbox()
    {
        if (UpAttackCollider != null)
        {
            UpAttackCollider.enabled = false;
        }

        if (SlashCollider != null)
        {
            SlashCollider.enabled = false;
        }

        if (StabCollider!= null)
        {
            StabCollider.enabled = false;
        }
    }

    // --- IA ---
    private void FinishAttack()
    {
        DisableHitbox();

        if (inputBuffered)
        {
            // Si on a bufférisé, on relance. 
            // Note: Ici tu pourrais ajouter une logique pour savoir si le prochain coup
            // doit être haut ou bas selon l'input maintenu.
            // Pour l'instant, on relance un combo standard par défaut.
            // The player pressed early (Buffer), so we chain immediately
            StartAttack();
        }
        else
        {
            // The player didn't press yet. 
            // We stop the attack state, BUT we don't reset comboCounter yet.
            // We record the time to allow the tolerance window in Update()
            IsAttacking = false;
            lastAttackEndTime = Time.time;
            isAttackingUp = false;
        }
    }
    private void TriggerAttackStep()
    {
        // Permit a step when attacking, only on ground and only when doing normal combo
        if (movement != null && !isAttackingUp)
        {
            movement.ApplyAttackStep(attackStepForce);
        }
    }
}

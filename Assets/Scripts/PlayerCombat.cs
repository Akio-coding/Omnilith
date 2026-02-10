using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Colliders d'attaque")]
    [SerializeField] private Collider2D SlashCollider;
    [SerializeField] private Collider2D StabCollider;

    [Header("Gestion timing")]
    [Tooltip("Nombre de coups maximum dans un combo")]
    [SerializeField] private int maxHitInCombo = 3;
    [Tooltip("Timing toléré après l'animation de l'attaque qui accepte le trigger Time allowed after an attack animation to still trigger the next combo step")]
    [SerializeField] private float comboTolerance = 0.5f;

    // --- Variables ---
    // -- State --

    private bool isAttacking = false;
    private bool inputBuffered = false;
    private int comboCounter = 0;
    private float lastAttackEndTime = 0f; // New: To track the tolerance window



    // -- General --
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Ensure that the sword collider is deactivated at the start
        DisableHitbox();
    }

    void Update()
   {
    // 1. Check for Timeout: Reset combo if user waited too long after the last attack
        if (!isAttacking && comboCounter > 0)
        {
            if (Time.time > lastAttackEndTime + comboTolerance)
            {
                comboCounter = 0; // Combo dropped
            }
        }

        // 2. Input Handling
        if (Input.GetButtonDown("Fire1"))
        {
            if (isAttacking)
            {
                // Case A: Player presses DURING animation -> Buffer the input
                if (comboCounter < maxHitInCombo)
                {
                    inputBuffered = true;
                }
            }
            else
            {
                // Case B: Player presses when NOT attacking (Start fresh OR Continue combo in tolerance window)
                StartAttack();
            }
        }
    }

    private void StartAttack()
    {
        // Reset logic
        inputBuffered = false;
        isAttacking = true;

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


    // --- FUNCTIONS CALLED BY THE ANIMATOR ---

    // 1. At the beginning of the movement (when the sword becomes dangerous)
    private void EnableHitbox()
    {
        if (comboCounter >= 3)
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

    // At the end of the movement (when we want to stop the attack) 
    private void DisableHitbox()
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

    // --- IA ---
    private void FinishAttack()
    {
        DisableHitbox();

        if (inputBuffered)
        {
            // The player pressed early (Buffer), so we chain immediately
            StartAttack();
        }
        else
        {
            // The player didn't press yet. 
            // We stop the attack state, BUT we don't reset comboCounter yet.
            // We record the time to allow the tolerance window in Update()
            isAttacking = false;
            lastAttackEndTime = Time.time;
        }
    }
}

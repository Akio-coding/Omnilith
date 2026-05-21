using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour
{
    [Header("Colliders d'attaque")]
    [SerializeField] private Collider2D SlashCollider;
    [SerializeField] private Collider2D StabCollider;
    [SerializeField] private Collider2D UpAttackCollider;
    [SerializeField] private Collider2D DownAttackCollider;

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
    private bool isAttackingDown = false;
    private bool inputBuffered = false;
    private int comboCounter = 0;
    private float lastAttackEndTime = 0f; // New: To track the tolerance window

    // -- General --
    private Animator anim;
    private PlayerMovement movement;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        IsAttacking = false;

        // Ensure that the sword collider is deactivated at the start
        DisableHitbox();

        // On s'abonne à l'événement de dégâts pour annuler l'attaque
        Health health = GetComponent<Health>();
        if(health != null )
        {
            health.OnDamageTaken += ResetCombatState;
        }
    }

    // Toujours se désabonner pour éviter les fuites de mémoire
    private void OnDestroy()
    {
        Health health = GetComponent<Health>();
        if(health != null)
        {
            health.OnDamageTaken -= ResetCombatState;
        }
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
            // NEW SECURITY: Prevent attack if the mouse is hovering over a UI element
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // We stop reading the function here, the attack is canceled
                return;
            }

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
                if (!movement.GetIsOnGround() && yInput < -0.1f)
                {
                    StartDownAttack();
                }
                // If we input up => UpAttack
                else if (yInput > 0.1f)
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
        isAttackingDown = false;

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
        isAttackingDown = false;

        // On ne touche pas au comboCounter pour l'attaque haute (souvent c'est un coup unique)
        // Ou tu peux décider qu'elle casse le combo, à toi de voir.
        if (!movement.GetIsOnGround())
        {
            anim.SetTrigger("AirAttackUp");
        }
        else
        {
            anim.SetTrigger("AttackUp");
        }
    }

    private void StartDownAttack()
    {
        inputBuffered = false;
        IsAttacking = true;
        isAttackingUp = false;
        isAttackingDown = true;

        anim.SetTrigger("AttackDown");
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
        else if (isAttackingDown)
        {
            if(DownAttackCollider != null)
            {
                DownAttackCollider.enabled = true;
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

        if (DownAttackCollider != null)
        {
            DownAttackCollider.enabled = false;
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
            isAttackingDown = false;
        }
    }
    private void TriggerAttackStep()
    {
        // Permit a step when attacking, only on ground and only when doing normal combo
        if (movement != null && !isAttackingUp && !isAttackingDown)
        {
            movement.ApplyAttackStep(attackStepForce);
        }
    }

    private void EndRecover()
    {
        anim.SetTrigger("Idle");
    }

    // Remet tout à zéro si on est interrompu
    private void ResetCombatState()
    {
        IsAttacking = false;
        isAttackingUp = false;
        isAttackingDown = false;
        inputBuffered = false;
        comboCounter = 0;

        // Très important pour éviter qu'une hitbox reste allumée pendant qu'on clignote !
        DisableHitbox() ;
    }
}

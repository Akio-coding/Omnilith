using UnityEngine;

public class FollowCameraTarget : MonoBehaviour
{
    [Header("Réglages")]
    [SerializeField] private Transform playerTransform; // Glisse ton Player ici
    [SerializeField] private float lookAheadAmount = 2f; // Distance du décalage
    [SerializeField] private float smoothTime = 0.2f; // Vitesse de transition

    private float currentXOffset;
    private float velocity; // For SmoothDamp

    void Update()
    {
        if (playerTransform == null) 
        { 
            return; 
        }

        // Detect the player's direction via his scale 
        // On détecte la direction du joueur via son échelle (si tu flip le sprite) 
        // ou via ton script PlayerMovement (facingDirection).
        // Ici, on regarde simplement l'échelle locale X du joueur ou sa rotation Y.
        // Adapté à ton script PlayerMovement qui utilise SpriteRenderer.flipX :

        float targetXOffset = 0f;

        // On récupère le SpriteRenderer du parent (Player) pour savoir si on regarde à gauche/droite
        SpriteRenderer sr = playerTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Si flipX est true (gauche), on vise -lookAheadAmount, sinon +lookAheadAmount
            targetXOffset = sr.flipX ? -lookAheadAmount : lookAheadAmount;
        }

        // Transition fluide (Lerp/SmoothDamp)
        currentXOffset = Mathf.SmoothDamp(currentXOffset, targetXOffset, ref velocity, smoothTime);

        // Appliquer la position : Position du joueur + décalage
        transform.position = new Vector3(playerTransform.position.x + currentXOffset, playerTransform.position.y, playerTransform.position.z);
    }
}

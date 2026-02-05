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

        float targetXOffset = 0f;

        // On regarde la direction "droite" (Right) locale du joueur.
        // Comme tu tournes le joueur de 180 degrés, son axe "Right" pointe vers la gauche du monde quand il se retourne.
        if (playerTransform.right.x > 0)
        {
            // L'axe rouge pointe vers la droite (Rotation Y = 0)
            targetXOffset = lookAheadAmount;
        }
        else
        {
            // L'axe rouge pointe vers la gauche (Rotation Y = 180)
            targetXOffset = -lookAheadAmount;
        }

        // Transition fluide (Lerp/SmoothDamp)
        currentXOffset = Mathf.SmoothDamp(currentXOffset, targetXOffset, ref velocity, smoothTime);

        // Appliquer la position : Position du joueur + décalage
        transform.position = new Vector3(playerTransform.position.x + currentXOffset, playerTransform.position.y, playerTransform.position.z);
    }
}

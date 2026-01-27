using UnityEngine;
using Unity.Cinemachine;

public class CameraFallingDamping : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Rigidbody2D playerRb; // Glisse ton Player ici
    [SerializeField] private CinemachineCamera cmCamera; // Glisse ta caméra ici

    [Header("Réglages Damping")]
    [SerializeField] private float risingDamping = 0.5f; // Lent quand on monte (défaut)
    [SerializeField] private float fallingDamping = 0.1f; // Rapide quand on tombe
    [SerializeField] private float threshold = -0.1f; // Vitesse Y pour considérer qu'on tombe

    private CinemachinePositionComposer positionComposer;

    void Start()
    {
        // En CM 3.x, les réglages de suivi sont dans le PositionComposer
        positionComposer = cmCamera.GetComponent<CinemachinePositionComposer>();

        if (positionComposer == null)
        {
            Debug.LogError("Pas de CinemachinePositionComposer trouvé sur la caméra ! Ajoutes-en un via l'inspecteur.");
        }
    }

    void Update()
    {
        if (playerRb == null || positionComposer == null) return;

        // Si on tombe (vitesse Y négative)
        if (playerRb.velocity.y < threshold)
        {
            // On réduit le damping Y pour suivre la chute rapidement
            // Lerp pour que le changement de valeur soit fluide
            positionComposer.Damping.y = Mathf.Lerp(positionComposer.Damping.y, fallingDamping, Time.deltaTime * 5f);
        }
        else
        {
            // On remet le damping normal
            positionComposer.Damping.y = Mathf.Lerp(positionComposer.Damping.y, risingDamping, Time.deltaTime * 5f);
        }
    }
}

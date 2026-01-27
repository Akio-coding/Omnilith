using UnityEngine;
using Unity.Cinemachine;

public class CameraTriggerZone : MonoBehaviour
{
    [Header("Caméra à activer")]
    [SerializeField] private CinemachineCamera cameraToActivate;

    // On garde une référence statique à la caméra active pour pouvoir la désactiver
    private static CinemachineCamera activeCamera;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Désactiver l'ancienne caméra (optionnel car Cinemachine gère les priorités, 
            // mais plus propre pour les perfs)
            if (activeCamera != null && activeCamera != cameraToActivate)
            {
                activeCamera.Priority = 0; // Baisse la priorité
                activeCamera.gameObject.SetActive(false);
            }

            // Activer la nouvelle
            if (cameraToActivate != null)
            {
                cameraToActivate.gameObject.SetActive(true);
                cameraToActivate.Priority = 10; // Monte la priorité
                activeCamera = cameraToActivate;
            }
        }
    }
}

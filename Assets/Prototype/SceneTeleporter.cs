using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Configuration de la Scène")]
    [Tooltip("Nom exact de la scène de destination")]
    public string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. On cherche le Rigidbody2D sur l'objet qui touche OU ses parents
        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();

        // 2. Si on a trouvé un Rigidbody et qu'il a le tag "Player"
        if (rb != null && rb.CompareTag("Player"))
        {
            Debug.Log($"Joueur détecté via : {collision.gameObject.name}. TP vers {sceneName}");
            Teleport();
        }
    }

    public void Teleport()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Attention : Le nom de la scène est vide sur l'objet " + gameObject.name);
        }
    }
}
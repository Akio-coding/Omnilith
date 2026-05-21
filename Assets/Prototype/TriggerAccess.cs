using UnityEngine;

public class TriggerAccess : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject objectToDestroy; // Le mur ou l'accès à faire disparaître
    public bool destroyPermanently = true; // Si faux, on le cache juste

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On utilise la même logique que ton TP : on cherche le parent "Player"
        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();

        if (rb != null && rb.CompareTag("Player"))
        {
            OpenAccess();
        }
    }

    private void OpenAccess()
    {
        if (objectToDestroy != null)
        {
            if (destroyPermanently)
            {
                Destroy(objectToDestroy);
            }
            else
            {
                objectToDestroy.SetActive(false); // Le cache seulement
            }

            Debug.Log("Accès ouvert !");
        }
        else
        {
            Debug.LogWarning("Aucun objet cible assigné au déclencheur.");
        }
    }
}

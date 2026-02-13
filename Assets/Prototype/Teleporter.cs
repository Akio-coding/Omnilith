using UnityEngine;

public class Teleporter : MonoBehaviour
{
    // On glissera l'objet "Destination" dans cette case via l'inspecteur
    public Transform destination;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("capter");

        // On vérifie si c'est bien le joueur qui entre en collision
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = destination.transform;
        }
        Debug.Log(collision.gameObject.tag);
    }
}
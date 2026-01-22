using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // L'accès universel

    [HideInInspector] public Vector3 respawnPoint; // La position mémorisée

    void Awake()
    {
        // Pattern Singleton classique : on s'assure qu'il n'y en a qu'un seul
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Garde le GM quand on change de scène (optionnel mais utile)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Au début du jeu, le point de respawn est la position de départ du joueur
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            respawnPoint = GameObject.FindGameObjectWithTag("Player").transform.position;
        }
    }

    public void UpdateCheckpoint(Vector3 newPosition)
    {
        respawnPoint = newPosition;
        Debug.Log("Checkpoint mis à jour !");
    }
}

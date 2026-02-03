using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Universal access

    [HideInInspector] public Vector3 respawnPoint; // Memorized position

    void Awake()
    {
        // Classic Singleton patern : ensure that there is only one 
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep the gameManager when we load an other scene (optionnal but usefull)
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // At the start of the game, the respawn point is player's starting position 
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

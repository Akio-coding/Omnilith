using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimedSceneTransition : MonoBehaviour
{
    [Header("Configuration du Temps")]
    [Tooltip("Temps d'attente en secondes avant de passer à la suite")]
    public float delayInSeconds = 3f;

    void Start()
    {
        // On lance le compte à rebours dès le début de la scène
        StartCoroutine(WaitAndLoadNextScene());
    }

    private IEnumerator WaitAndLoadNextScene()
    {
        // On attend le temps demandé (ex: 3 secondes)
        yield return new WaitForSeconds(delayInSeconds);

        // On récupère l'index de la scène suivante
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Sécurité : On vérifie que la scène suivante existe bien dans les Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogError("Erreur : Il n'y a pas de scène après celle-ci dans les Build Settings !");
        }
    }
}
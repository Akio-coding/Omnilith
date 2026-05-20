using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicZone : MonoBehaviour
{
    [Header("Réglages Audio")]
    public float maxVolume = 0.5f;       // Volume maximal de cette musique
    public float fadeDuration = 1.5f;    // Temps de transition (en secondes)

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Configuration automatique du composant AudioSource
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f; // On commence en silence
        audioSource.spatialBlend = 0f; // Mode 2D pour une musique d'ambiance globale
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie si c'est le joueur (avec ton système de parent/rigidbody)
        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();
        if (rb != null && rb.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // On lance le fondu en entrée (Fade In)
            StartFade(maxVolume);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();
        if (rb != null && rb.CompareTag("Player"))
        {
            // On lance le fondu en sortie (Fade Out)
            StartFade(0f, stopOnComplete: true);
        }
    }

    private void StartFade(float targetVolume, bool stopOnComplete = false)
    {
        // Si un fondu est déjà en cours, on l'arrête pour éviter les conflits
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeRoutine(targetVolume, stopOnComplete));
    }

    private IEnumerator FadeRoutine(float targetVolume, bool stopOnComplete)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        // Si on fait un Fade Out complet, on met la musique en pause/stop pour économiser les ressources
        if (stopOnComplete && targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }
}
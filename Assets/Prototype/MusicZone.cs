using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicZone : MonoBehaviour
{
    [Header("Réglages Audio")]
    public float maxVolume = 0.5f;
    public float fadeDuration = 1.5f;
    public string projectileLayerName = "Player Projectile";

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;
    private int playerCollidersInside = 0;

    // Au lieu d'une seule instance, on mémorise quelle zone est active par son NOM
    private static string activeZoneName = "";
    private static MusicZone activeInstance;

    void Awake()
    {
        // Si cette zone précise (par son nom) est déjà enregistrée comme active et vivante
        if (activeZoneName == gameObject.name && activeInstance != null && activeInstance != this)
        {
            // On vérifie si le joueur est à l'intérieur au respawn
            Collider2D playerCollider = CheckIfPlayerIsInside();
            if (playerCollider != null)
            {
                activeInstance.ForcePlayerInside(playerCollider);
            }

            // On détruit uniquement le doublon de CETTE zone après le respawn
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Configuration automatique
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(projectileLayerName)) return;

        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();
        if (rb != null && rb.CompareTag("Player"))
        {
            playerCollidersInside++;

            if (playerCollidersInside == 1)
            {
                // On devient la zone active et on devient indestructible
                activeZoneName = gameObject.name;
                activeInstance = this;
                DontDestroyOnLoad(gameObject);

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                StartFade(maxVolume);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(projectileLayerName)) return;

        Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();
        if (rb != null && rb.CompareTag("Player"))
        {
            playerCollidersInside--;

            if (playerCollidersInside <= 0)
            {
                playerCollidersInside = 0;
                StartFade(0f, stopOnComplete: true);

                // On n'est plus la zone active, on peut être détruit au prochain rechargement
                if (activeZoneName == gameObject.name)
                {
                    activeZoneName = "";
                    activeInstance = null;
                }
            }
        }
    }

    private void StartFade(float targetVolume, bool stopOnComplete = false)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
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

        if (stopOnComplete && targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }

    private Collider2D CheckIfPlayerIsInside()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return null;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Collider2D[] results = new Collider2D[10];
        int count = myCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Rigidbody2D rb = results[i].GetComponentInParent<Rigidbody2D>();
            if (rb != null && rb.CompareTag("Player")) return results[i];
        }
        return null;
    }

    public void ForcePlayerInside(Collider2D playerCollider)
    {
        if (playerCollidersInside == 0)
        {
            playerCollidersInside = 1;
            if (!audioSource.isPlaying) audioSource.Play();
            StartFade(maxVolume);
        }
    }
}
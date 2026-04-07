using System;
using System.Collections;
using UnityEngine;

public class Fallingplatforme : MonoBehaviour
{
    [Header("Réglages de Chute")]
    public float fallWait = 1.5f;        // Temps avant de tomber
    public float fallDuration = 2.0f;    // Temps avant que l'objet ne disparaisse après la chute

    [Header("Réglages de Réapparition")]
    public float respawnWait = 3.0f;     // Temps d'attente avant le début du retour
    public float fadeDuration = 1.0f;    // Durée du fondu (Alpha 0 -> 1)

    private bool isFalling = false;
    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        initialPosition = transform.position;
        originalColor = spriteRenderer.color;

        // On s'assure que la plateforme est fixe au début
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallAndRespawn());
        }
    }

    private IEnumerator FallAndRespawn()
    {
        isFalling = true;

        // 1. Attente avant la chute
        yield return new WaitForSeconds(fallWait);

        // 2. CHUTE : On active la physique
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 3. Attente pendant qu'elle tombe (elle est encore visible)
        yield return new WaitForSeconds(fallDuration);

        // 4. DISPARITION : On la rend invisible et on coupe les collisions
        spriteRenderer.enabled = false;
        platformCollider.enabled = false;

        // On la fige et on la remet à sa position d'origine (invisible)
        rb.bodyType = RigidbodyType2D.Static;
        rb.velocity = Vector2.zero;
        transform.position = initialPosition;

        // 5. ATTENTE : Avant la réapparition
        yield return new WaitForSeconds(respawnWait);

        // 6. RÉAPPARITION EN FONDU (Pas de blink)
        platformCollider.enabled = true; // On peut déjà marcher dessus
        spriteRenderer.enabled = true;   // On la réaffiche

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // On s'assure que la couleur est bien remise à 100% à la fin
        spriteRenderer.color = originalColor;
        isFalling = false;
    }
}
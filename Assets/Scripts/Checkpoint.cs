using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Sprite activeSprite; // Image du checkpoint activé (drapeau levé)
    [SerializeField] private Sprite inactiveSprite; // Image inactive

    private SpriteRenderer sr;
    private bool isActivated = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si le joueur touche le checkpoint et qu'il n'est pas déjà le point actif
        if (collision.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // 1. On change le visuel
        if (sr != null && activeSprite != null)
        {
            sr.sprite = activeSprite;
        }

        // 2. On prévient le chef (GameManager)
        GameManager.instance.UpdateCheckpoint(transform.position);
    }
}

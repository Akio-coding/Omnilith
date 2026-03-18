using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Ladders : MonoBehaviour
{

    private bool isInRange = false;
    public bool IsInRange { get; private set; }
    private PlayerMovement playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInRange == true)
        {
            playerMovement.IsClimbing = true;
        }
    }

    //Desactive la gravité quand le joueur touche l'échelle
    private void OnTriggerEnter2D(Collider2D PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D PlayerCollider)
    {
        if (PlayerCollider.CompareTag("Player"))
        {
            isInRange = false;
        }
    }

}

using Unity.VisualScripting;
using UnityEngine;

public class LevierFrappable : MonoBehaviour
{
    public GameObject porte;
    public Sprite levierActiveSprite;
    private bool IsActive = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox") && !IsActive)
        {
            ActionnerLeLevier();
        }
    }
    void ActionnerLeLevier()
    {
        IsActive = true;
        if (porte != null)
        {
            porte.SetActive(false);
        }
    }


    
}




using Unity.VisualScripting;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    [SerializeField] private GameObject GO_Door;
    [SerializeField] private Transform DoorOpening;
    private bool ActivatedLever = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Lorsque le joueur donne un coup d'épée sur le levier
    private void OnTriggerEnter2D(Collider2D PlayerAttack)
    {
        if (PlayerAttack.CompareTag("Sword"))
        {
            if (ActivatedLever == false)
            {
                ActivatedLever = true;
                GO_Door.transform.position = DoorOpening.transform.position;
            }
        }
    }
}

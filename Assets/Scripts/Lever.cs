using System.Collections;
using UnityEngine;

public class Lever : MonoBehaviour


{

    public GameObject Door;
    public Sprite levierActiveSprite;
    public bool IsActivated = false;
    public float HauteurCible = 5;
    public Vector3 OrginalDoorPosition;

    void Start()
    {
        OrginalDoorPosition = Door.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActivated)
        {
            StartCoroutine(OuvrirPorte());
        }
        

    }
    private IEnumerator OuvrirPorte()
    {
        while (Door.transform.position.y < OrginalDoorPosition.y + HauteurCible)
        {
            Door.transform.Translate(0.01f * Time.deltaTime * Vector3.up);
            yield return null;

        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            IsActivated = true;
            Debug.Log("Levier activé");
            
        }
    }



}

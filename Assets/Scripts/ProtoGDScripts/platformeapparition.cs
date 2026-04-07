using UnityEngine;

public class platformeapparition : MonoBehaviour
{
    public GameObject Door;
    public bool IsActivated = false;
    public Vector3 mouvement;
    public float mouvementSpeed = 2f;
    public bool apparait;

    private Vector3 PositionFinal;
    void Start()
    {
        PositionFinal = Door.transform.position + mouvement;
        if (apparait == true)
        {
            Door.transform.gameObject.SetActive(false);
            Door.gameObject.GetComponent<Collider2D>().gameObject.SetActive(false);
            //set sa collision a faux
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (IsActivated && (Door.transform.position != PositionFinal))
        {
            Door.transform.position = Vector3.MoveTowards(Door.transform.position, PositionFinal, Time.deltaTime * mouvementSpeed);

        }


    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hitbox"))
        {
            IsActivated = true;

            if (apparait == true)
            {
                Door.transform.gameObject.SetActive(true);
                //set sa collision a true
                Door.gameObject.GetComponent<Collider2D>().gameObject.SetActive(true);
            }
            Debug.Log("Levier activé");

        }
    }   // Start is called once before the first execution of Update after the MonoBehaviour is created
  
}

using UnityEngine;

public class DebugToolTp : MonoBehaviour
{
    [Header("Points de Téléportation")]
    [Tooltip("Glisse ici les objets vides (Transform) qui serviront de destination")]
    [SerializeField] private Transform[] teleportPoints;

    private Rigidbody2D rb;
    private TrailRenderer tr;

    private void Start()
    {
        // On récupère ces composants pour réinitialiser la physique et les effets lors du TP
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    private void Update()
    {
        // On vérifie les touches du pavé numérique.
        // La condition (teleportPoints.Length >= X) empêche le jeu de crasher 
        // si tu appuies sur Numpad 3 mais que tu n'as mis que 2 points dans ta liste !

        if (Input.GetKeyDown(KeyCode.Keypad1) && teleportPoints.Length >= 1)
        {
            TeleportTo(teleportPoints[0]);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2) && teleportPoints.Length >= 2)
        {
            TeleportTo(teleportPoints[1]);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3) && teleportPoints.Length >= 3) 
        { 
            TeleportTo(teleportPoints[2]); 
        }

        if (Input.GetKeyDown(KeyCode.Keypad4) && teleportPoints.Length >= 4) 
        { 
            TeleportTo(teleportPoints[3]); 
        }

        if (Input.GetKeyDown(KeyCode.Keypad5) && teleportPoints.Length >= 5)
        {
            TeleportTo(teleportPoints[4]);
        }

        if (Input.GetKeyDown(KeyCode.Keypad6) && teleportPoints.Length >= 6) 
        { 
            TeleportTo(teleportPoints[5]); 
        }

        if (Input.GetKeyDown(KeyCode.Keypad7) && teleportPoints.Length >= 7)
        {
            TeleportTo(teleportPoints[6]);
        }

        if (Input.GetKeyDown(KeyCode.Keypad8) && teleportPoints.Length >= 8)
        {
            TeleportTo(teleportPoints[7]);
        }

        if (Input.GetKeyDown(KeyCode.Keypad9) && teleportPoints.Length >= 9)
        { 
            TeleportTo(teleportPoints[8]); 
        }
    }

    private void TeleportTo(Transform targetPoint)
    {
        // Sécurité : si la case est vide, on annule
        if (targetPoint == null) 
        { 
            return; 
        }

        // 1. On déplace le joueur instantanément
        transform.position = targetPoint.position;

        // 2. On stoppe son élan (s'il tombait d'une falaise ou était en plein saut)
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // 3. BONUS PRO-TIP : On efface la traînée du Dash !
        // Si on ne fait pas ça, le TrailRenderer va dessiner une énorme ligne 
        // qui traverse toute la carte de ton ancienne position à ta nouvelle.
        if (tr != null)
        {
            tr.Clear();
        }

        Debug.Log("Téléporté avec succès vers : " + targetPoint.gameObject.name);
    }
}

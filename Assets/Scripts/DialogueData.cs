using UnityEngine;

// Cette ligne permet de créer ce fichier depuis le munu cllic droit unity

[CreateAssetMenu (fileName ="Nouveau Dialogue", menuName ="Mon Jeu/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName; // Nom de celui qui parle 
        [TextArea(3, 5)] // Aggrandit la zone de texte dans l'inspecteur
        public string texte; // Le texte du dialogue
    }

    [Header("Lignes de dialogues")]
    public DialogueLine[] lines;
}
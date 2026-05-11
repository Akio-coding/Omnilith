using UnityEngine;
using UnityEngine.UI; 

public class HeartDisplay : MonoBehaviour
{
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private Image heartImage;

    void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    // public because this function will be called by the manager 
    public void SetHeartStatus(bool isFull)
    {
        if (isFull)
        {
            heartImage.sprite = fullHeart;
        }
        else
        {
            heartImage.sprite = emptyHeart;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Health playerHealth; // Health script
    [SerializeField] private GameObject heartPrefab; // prefab of heart 
    [SerializeField] private Transform heartsContainer; // position where the hearts spawn (Grid/Horizontal Layout)

    private List<HeartDisplay> hearts = new List<HeartDisplay>();

    void Start()
    {
        // Subscribe to the event (delegate unreal)
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHearts;
        }

        // Initialisation : We force to update at the game start to display the full hearts 
        // We suppose that the health is full at the beginning 
        UpdateHearts(playerHealth.CurrentHealth, playerHealth.maxHealth); 
    }

    // Every time player take damage, this function is called 
    void UpdateHearts(int currentHealth, int maxHealth)
    {
        // If the max number of heart change (health upgrade), redo the list
        if (hearts.Count != maxHealth)
        {
            ClearHearts();
            CreateHearts(maxHealth);
        }

        // We examine all the list to determine their state
        for (int heartIndex = 0; heartIndex < hearts.Count; heartIndex++)
        {
            // If the index is lower than actual life, the heart is full
            // Exemple : Life = 3. 
            // Heart 0 (1er) < 3 -> Full
            // Heart 1 (2e)  < 3 -> Full
            // Heart 2 (3e)  < 3 -> Full
            // Heart 3 (4e)  >= 3 -> Empty
            if (heartIndex < currentHealth)
            {
                hearts[heartIndex].SetHeartStatus(true);
            }
            else
            {
                hearts[heartIndex].SetHeartStatus(false);
            }
        }
    }

    void CreateHearts(int amount)
    {
        for (int loopCount = 0; loopCount < amount; loopCount++)
        {
            // We create a new heart in the container
            GameObject newHeart = Instantiate(heartPrefab, heartsContainer);
            HeartDisplay heartScript = newHeart.GetComponent<HeartDisplay>();

            // We'll add it to our list so we can edit it later.
            hearts.Add(heartScript);
        }
    }

    void ClearHearts()
    {
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        hearts.Clear();
    }

    // Clean unsubscription to avoid errors if the ui is destroyed
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHearts;
        }
    }
}

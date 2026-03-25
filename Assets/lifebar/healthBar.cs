using UnityEngine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class healthBar : MonoBehaviour
{
    public Slider slider; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }
    public void SetHealth(int health)
    { 
        slider.value = health;
    }
}

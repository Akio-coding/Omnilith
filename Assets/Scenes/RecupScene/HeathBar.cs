using UnityEngine;
using UnityEngine.UI;
public class HeathBar : MonoBehaviour
{
    public Slider slider;
    public void SetmaxHeath(int health)
    {
        slider.maxValue = health;
        slider.value = 0;
    }
}

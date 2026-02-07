using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI Settings")]
    public Image healthBarFillSprite;

    private void OnEnable()
    {
        PlayerStatus.OnHealthUpdate += UpdateHealthBar;
    }

    private void OnDisable()
    {
        PlayerStatus.OnHealthUpdate -= UpdateHealthBar;
    }

    //Function to update health bar UI
    public void UpdateHealthBar(float healthPercent, ElementTypes.ElementType element)
    {
        //healthBarFillSprite.transform.localScale = new Vector3(healthPercent, 1, 1);
        healthBarFillSprite.fillAmount = healthPercent;
        Debug.Log("Health Updated: " + healthPercent);
    }
}

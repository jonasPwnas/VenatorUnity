using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Header("UI Settings")]
    public Image staminaBarFillSprite;

    private void OnEnable()
    {
        PlayerStatus.OnStaminaUpdate += UpdateStaminaBar;
    }

    private void OnDisable()
    {
        PlayerStatus.OnStaminaUpdate -= UpdateStaminaBar;
    }

    //Function to update health bar UI
    public void UpdateStaminaBar(float staminaPercent)
    {
        //healthBarFillSprite.transform.localScale = new Vector3(healthPercent, 1, 1);
        staminaBarFillSprite.fillAmount = staminaPercent;
        //Debug.Log("Stamina Updated: " + staminaPercent);
    }
}

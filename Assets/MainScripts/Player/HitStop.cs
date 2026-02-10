using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    [SerializeField] private float hitStopDuration = 0.1f;

    private void OnEnable()
    {
        PlayerHitboxHandler.OnPlayerDealtDamage += TriggerHitStop;
    }

    private void OnDisable()
    {
        PlayerHitboxHandler.OnPlayerDealtDamage -= TriggerHitStop;
    }

    public void TriggerHitStop(ElementTypes.ElementType elementType)
    {
        StartCoroutine(HitStopCoroutine());
    }

    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = 0f; // Pause the game
        yield return new WaitForSecondsRealtime(hitStopDuration); // Wait for the hit stop duration in real time
        Time.timeScale = 1f; // Resume the game
    }
}

using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{
    public Material NormalMaterial, LightAttackMaterial, HeavyAttackMatieal;

    public GameObject ai;


    public void PerformLightAttack()
    {
        StartCoroutine(LightAttack());
    }
    public void PerformHeavyAttack()
    {
        StartCoroutine(HeavyAttack());
    }
    private IEnumerator LightAttack()
    {
        ai.GetComponent<Renderer>().material = LightAttackMaterial;
        yield return new WaitForSeconds(0.5f);
        ai.GetComponent<Renderer>().material = NormalMaterial;

    }
    private IEnumerator HeavyAttack()
    {
        ai.GetComponent<Renderer>().material = HeavyAttackMatieal;
        yield return new WaitForSeconds(0.5f);
        ai.GetComponent<Renderer>().material = NormalMaterial;
    }
}

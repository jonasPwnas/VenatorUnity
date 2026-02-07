using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterExplosion : MonoBehaviour
{
    [SerializeField] GameObject destroyTargetObject;

    [SerializeField] private float waitTimer = 3f;

    public void StartDespawning()
    {
        StartCoroutine(DespawnObjects());
    }

    private IEnumerator DespawnObjects()
    {
        yield return new WaitForSeconds(waitTimer);

        Destroy(destroyTargetObject);
    }
}

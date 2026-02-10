using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> destroyableObjects;

    private void Start()
    {
        destroyableObjects = new List<GameObject>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit Tiles");

        foreach (var obj in destroyableObjects)
        {
            obj.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}

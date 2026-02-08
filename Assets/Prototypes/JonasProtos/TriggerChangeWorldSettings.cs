using System;
using UnityEngine;

public class TriggerChangeWorldSettings : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        WorldSettings.Instance.StartPostOnboarding(true);
    }
}

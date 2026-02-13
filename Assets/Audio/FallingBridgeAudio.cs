using FMODUnity;
using UnityEngine;

public class FallingBridgeAudio : MonoBehaviour
{
    [SerializeField] private EventReference Moving;

    public void MovingSfx()
    {
        RuntimeManager.PlayOneShot(Moving, transform.position);
    }
}

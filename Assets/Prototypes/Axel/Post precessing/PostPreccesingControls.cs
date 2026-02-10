using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostPreccesingControls : MonoBehaviour
{
    [SerializeField] private Volume PostprocessingVolume;
    [SerializeField, Range(0f,1f)] private float MotionBlurIntensity = 0.5f;

    private MotionBlur motionBlur;

    private void Start()
    {
        PostprocessingVolume.profile.TryGet(out motionBlur);
        ToggleMotionBlur(PlayerPrefs.GetInt("MotionBlur", 1) == 1);
    }
    //private void Update()
    //{
    // ToggleMotionBlur(PlayerPrefs.GetInt("MotionBlur", 1) == 1);
    //}
    public void ToggleMotionBlur(bool value)
    {
        motionBlur.intensity.value = value ? MotionBlurIntensity : 0f;
    }


}

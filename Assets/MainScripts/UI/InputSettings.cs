using Player;
using UnityEngine;
using UnityEngine.UIElements;

public class InputSettings : MonoBehaviour
{
    private Slider sensitivitySlider;
    public ThirdPersonCamera ThirdPersonCamera;

    private void Awake()
    {
        ThirdPersonCamera = FindAnyObjectByType<ThirdPersonCamera>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sensitivitySlider = GetComponent<Slider>();
        ThirdPersonCamera = FindAnyObjectByType<ThirdPersonCamera>();

        // Load the saved sensitivity setting from PlayerPrefs and apply it to the slider and camera
        float savedSensitivity = PlayerPrefs.GetFloat("CameraSensitivity", ThirdPersonCamera.m_cameraRotSensitivity); // Default to 1.0f if not set
        sensitivitySlider.value = savedSensitivity;
        ThirdPersonCamera.m_cameraRotSensitivity = savedSensitivity;
    }

    public void SetSensitivity(float value)
    {
        ThirdPersonCamera.m_cameraRotSensitivity = value;
        PlayerPrefs.SetFloat("CameraSensitivity", value); // Save the sensitivity setting using PlayerPrefs
    }
}

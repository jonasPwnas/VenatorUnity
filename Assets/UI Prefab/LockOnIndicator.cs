using Player;
using UnityEngine;

public class LockOnIndicator : MonoBehaviour
{
    private Camera m_camera;
    private IndicatorPositioner m_positioner;
    
    private void Awake()
    {
        m_camera = Camera.main;
        m_positioner = GetComponentInChildren<IndicatorPositioner>();
        m_positioner.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerController.OnLockOnModeUpdate += ToggleIndicator;
    }

    private void OnDisable()
    {
        PlayerController.OnLockOnModeUpdate -= ToggleIndicator;
    }

    private void ToggleIndicator(bool enable, Transform target)
    {
        if (enable)
        {
            Transform offset = target.gameObject.GetComponent<ILockOnAble>().GetLockOnTarget();
            m_positioner.EnableAndSetTarget(offset, m_camera);
        }
        else
        {
            m_positioner.gameObject.SetActive(false);
        }
    }
}
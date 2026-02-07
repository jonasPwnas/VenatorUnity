using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GoapPerceptionHub : MonoBehaviour
{
    [Header("References")]
    [Tooltip("If empty, will auto-find on this GameObject.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Sensors")]
    [Tooltip("Add sensor components here (Sight, Hearing, Combat, Nav...). Leave out for disabled senses (eg deaf enemy).")]
    [SerializeField] private List<MonoBehaviour> m_sensorBehaviours = new List<MonoBehaviour>();

    [Tooltip("If true, will auto-find all IGoapSensor components on this GameObject.")]
        [SerializeField] private bool m_autoFindSensors = true;

    [Header("Memory")]
    [Tooltip("Seconds to keep TargetKnown after last seen/heard.")]
    [SerializeField] private float m_targetKnownMemorySeconds = 6f;

    [Tooltip("Seconds to keep TargetVisible after last seen (anti-flicker).")]
    [SerializeField] private float m_targetVisibleGraceSeconds = 0.15f;

    [Header("Range Buckets")]
    [Tooltip("Distance <= this is melee (TargetRange=3).")]
    [SerializeField] private float m_meleeDistance = 2.2f;

    [Tooltip("Distance <= this is chase (TargetRange=2). Above becomes far (1).")]
    [SerializeField] private float m_chaseDistance = 9f;

    [Header("Write Keys")]
    [Tooltip("If true, hub will write perception keys every frame.")]
    [SerializeField] private bool m_isEnabled = true;

    private IGoapSensor[] m_sensors;
    private GoapPerceptionBlackboard m_blackboard;

    private float m_lastSeenTime = -9999f;
    private float m_lastKnownTime = -9999f;
    private Vector3 m_lastKnownPosition;



    private void Awake()
    {
        AutoGetReferencesIfMissing();
        CacheSensors();
        m_blackboard = new GoapPerceptionBlackboard();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnEnable()
{
    CacheSensors();
}

    private void Update()
    {
        if (!m_isEnabled)
            return;

        if (m_worldStateBuilder == null)
            return;

        EvaluatePerception();
    }

    private void EvaluatePerception()
    {
        m_blackboard.ResetFrame();

        if (m_sensors != null)
        {
            for (int i = 0; i < m_sensors.Length; i++)
                m_sensors[i].Collect(m_blackboard);
        }

        float timeNow = Time.time;

        bool targetVisibleNow = m_blackboard.TargetVisible;
        bool targetKnownNow = m_blackboard.TargetKnown;

        if (targetVisibleNow)
        {
            m_lastSeenTime = timeNow;
            m_lastKnownTime = timeNow;
            m_lastKnownPosition = m_blackboard.VisibleTargetPosition;
        }
        else if (targetKnownNow)
        {
            m_lastKnownTime = timeNow;
            m_lastKnownPosition = m_blackboard.KnownTargetPosition;
        }

        bool targetVisibleWithGrace = targetVisibleNow || (timeNow - m_lastSeenTime) <= m_targetVisibleGraceSeconds;
        bool targetKnownWithMemory = targetKnownNow || (timeNow - m_lastKnownTime) <= m_targetKnownMemorySeconds;

        int alerted = targetKnownWithMemory ? 1 : 0;

        int targetRange = 0;

        if (targetVisibleWithGrace && m_blackboard.HasDistanceToTarget)
        {
            float d = m_blackboard.DistanceToTarget;

            if (d <= m_meleeDistance)
                targetRange = 3;
            else if (d <= m_chaseDistance)
                targetRange = 2;
            else
                targetRange = 1;
        }
        else if (targetKnownWithMemory)
        {
            targetRange = 2;
        }

m_worldStateBuilder.SetIntIfChanged(GoapStateKey.Alerted, alerted);
m_worldStateBuilder.SetIntIfChanged(GoapStateKey.TargetKnown, targetKnownWithMemory ? 1 : 0);
m_worldStateBuilder.SetIntIfChanged(GoapStateKey.TargetVisible, targetVisibleWithGrace ? 1 : 0);
m_worldStateBuilder.SetIntIfChanged(GoapStateKey.TargetRange, targetRange);
m_worldStateBuilder.SetIntIfChanged(GoapStateKey.HasPathToTarget, m_blackboard.HasPathToTarget ? 1 : 0);
m_worldStateBuilder.SetIntIfChanged(GoapStateKey.TargetAttackingNow, m_blackboard.TargetAttackingNow ? 1 : 0);


        if (targetKnownWithMemory)
            m_worldStateBuilder.SetLastKnownTargetPosition(m_lastKnownPosition);
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
    }

    private void CacheSensors()
{
        List<IGoapSensor> list = new List<IGoapSensor>(16);

        if (m_autoFindSensors)
        {
            MonoBehaviour[] mbs = GetComponents<MonoBehaviour>();
            for (int i = 0; i < mbs.Length; i++)
            {
                if (mbs[i] is IGoapSensor sensor)
                    list.Add(sensor);
            }
        }
        else
        {
            if (m_sensorBehaviours != null)
            {
                for (int i = 0; i < m_sensorBehaviours.Count; i++)
                {
                    MonoBehaviour mb = m_sensorBehaviours[i];
                    if (mb is IGoapSensor sensor)
                        list.Add(sensor);
                }
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (!m_sensorBehaviours.Contains(list[i] as MonoBehaviour))
                m_sensorBehaviours.Add(list[i] as MonoBehaviour);
        }

            m_sensors = list.ToArray();
    }


}

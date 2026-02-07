using UnityEngine;

public class GoapPerceptionBlackboard
{
    public bool TargetVisible => m_targetVisible;
    public bool TargetKnown => m_targetKnown;
    public bool HasPathToTarget => m_hasPathToTarget;
    public bool TargetAttackingNow => m_targetAttackingNow;
    public bool HasDistanceToTarget => m_hasDistanceToTarget;

    public float DistanceToTarget => m_distanceToTarget;

    public Vector3 VisibleTargetPosition => m_visibleTargetPosition;
    public Vector3 KnownTargetPosition => m_knownTargetPosition;

    private bool m_targetVisible;
    private float m_targetVisibleConfidence;
    private Vector3 m_visibleTargetPosition;
    private float m_distanceToTarget;
    private bool m_hasDistanceToTarget;

    private bool m_targetKnown;
    private float m_targetKnownConfidence;
    private Vector3 m_knownTargetPosition;

    private bool m_hasPathToTarget;
    private float m_hasPathConfidence;

    private bool m_targetAttackingNow;
    private float m_targetAttackingConfidence;

    public void ResetFrame()
    {
        m_targetVisible = false;
        m_targetVisibleConfidence = 0f;
        m_visibleTargetPosition = Vector3.zero;
        m_distanceToTarget = 0f;
        m_hasDistanceToTarget = false;

        m_targetKnown = false;
        m_targetKnownConfidence = 0f;
        m_knownTargetPosition = Vector3.zero;

        m_hasPathToTarget = false;
        m_hasPathConfidence = 0f;

        m_targetAttackingNow = false;
        m_targetAttackingConfidence = 0f;
    }

    public void ReportVisibleTarget(float confidence01, Vector3 position, float distance)
    {
        float c = Mathf.Clamp01(confidence01);

        if (c <= m_targetVisibleConfidence)
            return;

        m_targetVisible = true;
        m_targetVisibleConfidence = c;
        m_visibleTargetPosition = position;

        m_distanceToTarget = Mathf.Max(0f, distance);
        m_hasDistanceToTarget = true;

        ReportKnownTarget(c, position);
    }

    public void ReportKnownTarget(float confidence01, Vector3 position)
    {
        float c = Mathf.Clamp01(confidence01);

        if (c <= m_targetKnownConfidence)
            return;

        m_targetKnown = true;
        m_targetKnownConfidence = c;
        m_knownTargetPosition = position;
    }

    public void ReportHasPathToTarget(float confidence01, bool hasPath)
    {
        float c = Mathf.Clamp01(confidence01);

        if (c <= m_hasPathConfidence)
            return;

        m_hasPathConfidence = c;
        m_hasPathToTarget = hasPath;
    }

    public void ReportTargetAttackingNow(float confidence01, bool isAttackingNow)
    {
        float c = Mathf.Clamp01(confidence01);

        if (c <= m_targetAttackingConfidence)
            return;

        m_targetAttackingConfidence = c;
        m_targetAttackingNow = isAttackingNow;
    }
}






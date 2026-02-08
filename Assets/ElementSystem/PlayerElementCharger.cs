using System;
using System.Collections;
using UnityEngine;

public class PlayerElementCharger : MonoBehaviour
{
    //delegates blooppppp
    public delegate void FinishedCharging();
    public static event FinishedCharging OnFinishedCharging;

    //editor exposed
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private ParticleSystem fireChargeEffect;
    [SerializeField] private ParticleSystem waterChargeEffect;

    private SourceVFXManager m_targetSourceLogic;
    private IncreaseEmissionIntensity m_eyeLightIntensity;
    private IDamageableElementalSource m_currentSourceElement;
    private SelectionWheel m_selectionWheel;

    private bool m_chargingAir;
    
    //private
    private Collider m_collider;
    private IDamageableElementalSource m_source;
    
    private void Awake()
    {
        m_collider = GetComponent<Collider>();
        gameObject.SetActive(false);
        m_selectionWheel = FindAnyObjectByType<SelectionWheel>();
    }

    public void EnableCharger(bool enable)
    {
        if (enable)
        {
            gameObject.SetActive(enable);
            StartCoroutine(AirChargeWait(0.4f));
        }
        else
        {
            if (!m_chargingAir)
            {
                switch (m_currentSourceElement.GetSourceElement())
                {
                    case ElementTypes.ElementType.Fire:
                        m_targetSourceLogic.DecreaseSize();
                        m_eyeLightIntensity.increase = false;
                        break;
                    case ElementTypes.ElementType.Water:
                        m_eyeLightIntensity.increase = false;
                        break;
                }
            }         

            StopAllCoroutines();

            gameObject.SetActive(enable);
        }
    }

    private void OnDisable()
    {
        m_collider.enabled = true;
        m_source = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageableElementalSource>() != null)
        {
            m_source = other.gameObject.GetComponent<IDamageableElementalSource>();

            m_currentSourceElement = other.gameObject.GetComponent<IDamageableElementalSource>();

            m_targetSourceLogic = other.gameObject.GetComponent<SourceVFXManager>();

            m_eyeLightIntensity = other.gameObject.GetComponent<IncreaseEmissionIntensity>();

            m_chargingAir = false;

            switch (m_source.GetSourceElement())
            {
                case ElementTypes.ElementType.Fire:

                    m_targetSourceLogic.IncreaseSize();

                    m_eyeLightIntensity.active = true;
                    m_eyeLightIntensity.increase = true;

                    StartCoroutine(ChargeFireOrWater(m_source));

                    break;
                case ElementTypes.ElementType.Water:

                    m_eyeLightIntensity.active = true;
                    m_eyeLightIntensity.increase = true;

                    StartCoroutine(ChargeFireOrWater(m_source));

                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StopAllCoroutines();
    }

    IEnumerator AirChargeWait(float airWaitTime)
    {
        yield return new WaitForSeconds(airWaitTime);
        if (m_source == null)
        {
            m_chargingAir = true;
            m_collider.enabled = false;
            StartCoroutine(ChargeAir(6f));
        }
    }

    IEnumerator ChargeAir(float chargeDuration)
    {
        m_selectionWheel.SwitchToChargingElement(ElementTypes.ElementType.Air);
        yield return new WaitForSeconds(chargeDuration * 0.333f);
        playerStatus.AddElement(ElementTypes.ElementType.Air, 1, true);

        yield return new WaitForSeconds(chargeDuration * 0.333f);
        playerStatus.AddElement(ElementTypes.ElementType.Air, 1, true);

        yield return new WaitForSeconds(chargeDuration * 0.333f);
        playerStatus.AddElement(ElementTypes.ElementType.Air, 1, true);

        OnFinishedCharging?.Invoke();
        gameObject.SetActive(false);
    }

    IEnumerator ChargeFireOrWater(IDamageableElementalSource source)
    {
        m_selectionWheel.SwitchToChargingElement(source.GetSourceElement());
        yield return new WaitForSeconds(source.ChargeElementTime() * 0.333f);
        playerStatus.AddElement(source.GetSourceElement(), 1, true);

        yield return new WaitForSeconds(source.ChargeElementTime() * 0.333f);
        playerStatus.AddElement(source.GetSourceElement(), 1, true);

        yield return new WaitForSeconds(source.ChargeElementTime() * 0.333f);
        playerStatus.AddElement(source.GetSourceElement(), 1, true);

        OnFinishedCharging?.Invoke();
        gameObject.SetActive(false);
    }
}

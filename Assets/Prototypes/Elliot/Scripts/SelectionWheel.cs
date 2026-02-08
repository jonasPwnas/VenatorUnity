using System.Collections.Generic;
using Input;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UxPrototype;

public class SelectionWheel : MonoBehaviour
{
    //delegates
    public delegate void ChangedElement(ElementTypes.ElementType newElement);
    public static event ChangedElement OnElementChanged;
    
    //Editor expose
    [Header("Charge Glow")]
    public Sprite chargeFire;
    public Sprite chargeWater;
    public Sprite chargeAir;
    public Sprite chargeNone;

    [Header("Slow Motion Settings")]
    public float slowedTimeScale = 0.5f;
    public  float normalTimeScale = 1f;
    [HideInInspector] public ElementTypes.ElementType currentElement;

    PauseManager pauseManager;
    PlayerInput playerInput;
    [SerializeField] private GameObject chargeContainer;
    ChargeManager chargeManager;
    ChargeDebugger chargeDebugger;
    PlayerStatus playerStatus;
    ElementSelector elementSelector;

    private void Awake()
    {
        pauseManager = FindAnyObjectByType<PauseManager>();
        playerInput = this.GetComponent<PlayerInput>();
        chargeManager = FindAnyObjectByType<ChargeManager>();
        chargeDebugger = FindAnyObjectByType<ChargeDebugger>();
        playerStatus = FindAnyObjectByType<PlayerStatus>();
        elementSelector = FindAnyObjectByType<ElementSelector>();
    }

    private void Start()
    {
        NoElement();
        playerInput.SwitchCurrentActionMap("Player");
    }

    private void OnEnable()
    {
        PlayerStatus.OnElementStackChanged += UpdateElements;
    }

    private void OnDisable()
    {
        PlayerStatus.OnElementStackChanged -= UpdateElements;
    }
    
    private void UpdateElements(ElementTypes.ElementType elementToUpdate, int stack)
    {
        switch(elementToUpdate)
        {
            case ElementTypes.ElementType.Fire:
                FireElement(); 
                break;
            case ElementTypes.ElementType.Water:
                WaterElement();
                break;
            case ElementTypes.ElementType.Air:
                AirElement();
                break;
            case ElementTypes.ElementType.NoElement:
                NoElement();
                break;
        }
    }

    public void FireElement()
    {
        currentElement = ElementTypes.ElementType.Fire;
        chargeContainer.SetActive(true);
        OnElementChanged?.Invoke(currentElement);
    }

    public void WaterElement()
    {
        currentElement = ElementTypes.ElementType.Water;
        chargeContainer.SetActive(true);
        OnElementChanged?.Invoke(currentElement);
    }

    public void AirElement()
    {
        currentElement = ElementTypes.ElementType.Air;
        chargeContainer.SetActive(true);
        OnElementChanged?.Invoke(currentElement);
    }

    public void NoElement()
    {
        currentElement = ElementTypes.ElementType.NoElement;
        chargeContainer.SetActive(true);
        OnElementChanged?.Invoke(currentElement);
    }
    
    public void OnNextElement()
    {
        if (pauseManager.isPaused)
        {
            return;
        }
        
        elementSelector.ChangeSelection(1);
        
        switch (currentElement)
        {
            case ElementTypes.ElementType.Fire:
                WaterElement();
                break;
            case ElementTypes.ElementType.Water:
                AirElement();
                break;
            case ElementTypes.ElementType.Air:
                NoElement();
                break;
            case ElementTypes.ElementType.NoElement:
                FireElement();
                break;
        }
    }

    public void OnPreviousElement()
    {
        if (pauseManager.isPaused)
        {
            return;
        }
        
        elementSelector.ChangeSelection(-1);
        
        switch (currentElement)
        {
            case ElementTypes.ElementType.Fire:
                NoElement();
                break;
            case ElementTypes.ElementType.Water:
                FireElement();
                break;
            case ElementTypes.ElementType.Air:
                WaterElement();
                break;
            case ElementTypes.ElementType.NoElement:
                AirElement();
                break;
        }
    }

    public void SwitchToChargingElement(ElementTypes.ElementType chargingElement)
    {
        elementSelector.SwitchToChargingElement(chargingElement);
    }
}

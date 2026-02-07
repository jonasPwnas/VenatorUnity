using System;
using Player;
using UnityEngine;

public class PlayerStatus : MonoBehaviour, IDamageableElementalSource
{
    //delegates
    public delegate void ElementStackChange(ElementTypes.ElementType elementType, int newStackAmount);
    public static event ElementStackChange OnElementStackChanged;

    public delegate void ElementEquipped(ElementTypes.ElementType elementType, int currentStack);
    public static event ElementEquipped OnElementEquipped;

    public delegate void TookDamage(float damageAmount, ElementTypes.ElementType elementType);
    public static event TookDamage OnTookDamage;

    public delegate void Healed(float healAmount, ElementTypes.ElementType elementType);
    public static event Healed OnHealed;

    public delegate void HealthUpdate(float currentHealthPercentage, ElementTypes.ElementType elementType);
    public static event HealthUpdate OnHealthUpdate;
    
    public delegate void StaminaUpdate(float staminaPercentage);
    public static event StaminaUpdate OnStaminaUpdate;

    

    public delegate void PlayerDied();
    public static event PlayerDied OnPlayerDied;

    public struct StaminaReservation
    {
        public float requestedActionCost;   // actuall cost of action
        public float reservedAmount;        // the was actually reserved (<= cost)
        public float overdraftAmount;       // cost - reserved (>= 0)
        public bool wouldOverdraw => overdraftAmount > 0f;
        public bool IsValid => requestedActionCost > 0f && reservedAmount > 0f;
    }

    //Editor exposed
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [Header("Elements")]
    [SerializeField] private int maxElementStack = 3;
    [SerializeField] private bool infiniteElementStack = false;
    [SerializeField] private ParticleSystem fireSystem;
    [SerializeField] private ParticleSystem waterSystem;
    [SerializeField] private ParticleSystem windSystem;
    [Header("Damage multipliers on interacting elements")]
    [SerializeField] public float strongElementMultiplier = 2f;
    [SerializeField] public float weakElementMultiplier = 0.25f;
    [SerializeField] public float sameElementMultiplier = 0.5f;
    [SerializeField] public float noElementMultiplier = 0.5f;
    
    //Private
    private ElementTypes.ElementType m_equippedElementType;
    private bool m_isDead;
    private float m_currentHealth;
    private float m_currentStamina;
    private float m_reservedStamina;
    private int m_fireAmount;
    private int m_airAmount;
    private int m_waterAmount;
    private int m_earthAmount;
    private bool m_isBlocking;

    // NEW
    private bool m_hasIFrames;

    public void SetNotDead()
    {
        m_isDead = false;
    }

    private void Start()
    {
        m_currentHealth = maxHealth;
        m_currentStamina = maxStamina;
        if (infiniteElementStack)
        {
            m_fireAmount = 3;
            m_waterAmount = 3;
            m_airAmount = 3;
            AddElement(ElementTypes.ElementType.Water, m_fireAmount);
            AddElement(ElementTypes.ElementType.Air, m_airAmount);
            AddElement(ElementTypes.ElementType.Water, m_waterAmount);
        }
    }

    private void OnEnable()
    {
        SelectionWheel.OnElementChanged += SetEquippedElement;
        PlayerController.OnBlocking += UpdateBlocking;
        PlayerController.OnToggleIframes += UpdateIframes;
    }

    private void OnDisable()
    {
        SelectionWheel.OnElementChanged -= SetEquippedElement;
        PlayerController.OnBlocking -= UpdateBlocking;
        PlayerController.OnToggleIframes -= UpdateIframes;
    }


    private void UpdateBlocking(bool isBlocking)
    {
        m_isBlocking = isBlocking;
    }

    private void UpdateIframes(bool hasIFrames)
    {
        m_hasIFrames = hasIFrames;
        Debug.Log($"Iframes status = {m_hasIFrames}");
    }

    private void SetEquippedElement(ElementTypes.ElementType newElement)
    {
        m_equippedElementType = newElement;
        switch (newElement)
        {
            case ElementTypes.ElementType.Water:
                if (m_waterAmount > 0)
                {
                    waterSystem.gameObject.SetActive(true);
                }
                fireSystem.gameObject.SetActive(false);
                windSystem.gameObject.SetActive(false);
                OnElementEquipped?.Invoke(newElement, m_waterAmount);
                break;
            case ElementTypes.ElementType.Air:
                if (m_airAmount > 0)
                {
                    windSystem.gameObject.SetActive(true);
                }
                fireSystem.gameObject.SetActive(false);
                waterSystem.gameObject.SetActive(false);
                OnElementEquipped?.Invoke(newElement, m_airAmount);
                break;
            case ElementTypes.ElementType.Fire:
                if (m_fireAmount > 0)
                {
                    fireSystem.gameObject.SetActive(true);
                }
                windSystem.gameObject.SetActive(false);
                waterSystem.gameObject.SetActive(false);
                OnElementEquipped?.Invoke(newElement, m_fireAmount);
                break;
            case ElementTypes.ElementType.NoElement:
                fireSystem.gameObject.SetActive(false);
                windSystem.gameObject.SetActive(false);
                waterSystem.gameObject.SetActive(false);
                break;
        }
    }

    private float GetDamageMultiplier(ElementTypes.ElementType damageType)
    {
        float multiplier = 0.5f;

        switch (GetSourceElement()) 
        {
            case ElementTypes.ElementType.Fire: //player element
                switch (damageType) //enemy element
                {
                    case ElementTypes.ElementType.Fire:
                        return sameElementMultiplier;
                    case ElementTypes.ElementType.Water:
                        return strongElementMultiplier;
                    case ElementTypes.ElementType.Air:
                        return weakElementMultiplier;
                    case ElementTypes.ElementType.NoElement:
                        return noElementMultiplier;
                }
                break;
            case ElementTypes.ElementType.Water: //player element
                switch (damageType) //enemy element
                {
                    case ElementTypes.ElementType.Fire:
                        return weakElementMultiplier;
                    case ElementTypes.ElementType.Water:
                        return sameElementMultiplier;
                    case ElementTypes.ElementType.Air:
                        return strongElementMultiplier;
                    case ElementTypes.ElementType.NoElement:
                        return noElementMultiplier;
                }
                break;
            case ElementTypes.ElementType.Air: //player element
                switch (damageType) //enemy element
                {
                    case ElementTypes.ElementType.Fire:
                        return strongElementMultiplier;
                    case ElementTypes.ElementType.Water:
                        return weakElementMultiplier;
                    case ElementTypes.ElementType.Air:
                        return sameElementMultiplier;
                    case ElementTypes.ElementType.NoElement:
                        return noElementMultiplier;
                }
                break;
            case ElementTypes.ElementType.NoElement:
                return strongElementMultiplier;
        }
        return multiplier;
    }
    

    public void AddStamina(float addAmount)
    {
        m_currentStamina = Mathf.Clamp(m_currentStamina + addAmount, 0, maxStamina);
        m_reservedStamina = Mathf.Min(m_reservedStamina, m_currentStamina);

        OnStaminaUpdate?.Invoke(GetCurrentStaminaPerc());
    }
    
    public void RemoveStamina(float addAmount)
    {
        m_currentStamina = Mathf.Clamp(m_currentStamina - addAmount, 0, maxStamina);

        OnStaminaUpdate?.Invoke(GetCurrentStaminaPerc());
    }

    // NOTE: If anything external should remove stamina from the character directly, use this method instead of RemoveStamina()
    //       I guess this was a bank after all: escrow-safe drains...
    public void RemoveStaminaNonReserved(float amount)
    {
        m_currentStamina = Mathf.Clamp(m_currentStamina - amount, m_reservedStamina, maxStamina);
        OnStaminaUpdate?.Invoke(GetCurrentStaminaPerc());
    }

    public bool TryReserveStamina(float requestedCost, out StaminaReservation res)
    {
        res = default;
        if (requestedCost <= 0f) return true; // no need to calculate anything, and some actions might be free under special circumstances

        float available = m_currentStamina - m_reservedStamina;
        if (available <= 0.0001f) return false;     // could shake stamina bar and flash it red here, to show requested action not permitted 

        // When only little stamina remains, allow actions that would overdraw, but don't make stamina actually go into negatives
        // If the cost of the action is more than what stammina was left, mark the overdraft value 

        float reserved = Mathf.Min(requestedCost, available);

        res.requestedActionCost = requestedCost;
        res.reservedAmount = reserved; 
        res.overdraftAmount = requestedCost - res.reservedAmount;
        
        m_reservedStamina = Mathf.Clamp(m_reservedStamina + reserved, 0f, maxStamina);
        return true;
    }

    public void CommitReservedStamina(in StaminaReservation res)
    {
        if (res.requestedActionCost <= 0f) return;

        m_reservedStamina = Mathf.Max(0f, m_reservedStamina - res.reservedAmount);
        RemoveStamina(res.reservedAmount);
    }

    public void ReleaseReservedStamina(in StaminaReservation res)
    {
        if (res.requestedActionCost <= 0f) return;

        m_reservedStamina = Mathf.Max(0f, m_reservedStamina - res.reservedAmount); 
    }
    
    public float GetCurrentStaminaPerc()
    {
        return maxStamina <= 0f ? 0f : (m_currentStamina / maxStamina);
    }

    public float GetAvailableStaminaPerc()
    {
        return maxStamina <= 0f ? 0f : Mathf.Clamp01((m_currentStamina - m_reservedStamina) / maxStamina);
    }


    private float GetCurrentHealthPerc()
    {
        return m_currentHealth / maxHealth;
    }

    // NEW
    public bool HasIFrames() => m_hasIFrames;

    public float ChargeElementTime()
    {
        //no
        return 0f;
    }

    public int GetCurrentElementAmount()
    {
        switch (GetSourceElement())
        {
            case ElementTypes.ElementType.Water:
                return m_waterAmount;
            case ElementTypes.ElementType.Air:
                return m_airAmount;
            case ElementTypes.ElementType.Fire:
                return m_fireAmount;
            case ElementTypes.ElementType.NoElement:
                return 0;
        }
        return 0;
    }
    
    public void TakeDamage(float damageAmount, ElementTypes.ElementType elementType)
    {
        if (m_isBlocking || m_hasIFrames)
            return; //Do more fancy stuff here soon :')
        
        m_currentHealth -= damageAmount * GetDamageMultiplier(elementType);
        
        OnHealthUpdate?.Invoke(GetCurrentHealthPerc(), elementType);

        if (m_currentHealth <= 0)
        {
            m_isDead = true;
            OnPlayerDied?.Invoke();
            return;
        }

        OnTookDamage?.Invoke(damageAmount, elementType);
        switch (elementType) //Do damage effects here?
        {
            case ElementTypes.ElementType.Fire:
                break;
            case ElementTypes.ElementType.Water:
                break;
            case ElementTypes.ElementType.Air:
                break;
            case ElementTypes.ElementType.Earth:
                break;
        }
    }

    public void Heal(float healAmount, ElementTypes.ElementType elementType)
    {
        OnHealed?.Invoke(healAmount, elementType);
        m_currentHealth += healAmount;
        m_currentHealth = Mathf.Clamp(m_currentHealth, 0, maxHealth);
        OnHealthUpdate?.Invoke(GetCurrentHealthPerc(), elementType);

        switch (elementType) //Do healing effects here?
        {
            case ElementTypes.ElementType.Fire:
                break;
            case ElementTypes.ElementType.Water:
                break;
            case ElementTypes.ElementType.Air:
                break;
            case ElementTypes.ElementType.Earth:
                break;
        }
    }

    public IDamageableElementalSource.ElementInteractType InteractType()
    {
        return IDamageableElementalSource.ElementInteractType.User;
    }

    public int EquippedElementAmount()
    {
        return GetCurrentElementAmount();
    }

    public int ElementAmountToGive()
    {
        return 0; //could allow enemies to steal elements from player?
    }

    public ElementTypes.ElementType GetSourceElement()
    {
        return m_equippedElementType;
    }

    public void AddElement(ElementTypes.ElementType elementType, int amount)
    {
        if (infiniteElementStack)
        {
            switch (elementType)
            {
                case ElementTypes.ElementType.Fire:
                    m_fireAmount += amount;
                    m_fireAmount = Mathf.Clamp(m_fireAmount, 0, maxElementStack);
                    OnElementStackChanged?.Invoke(elementType, m_fireAmount);
                    break;
                case ElementTypes.ElementType.Water:
                    m_waterAmount += amount;
                    m_waterAmount = Mathf.Clamp(m_waterAmount, 0, maxElementStack);
                    OnElementStackChanged?.Invoke(elementType, m_waterAmount);
                    break;
                case ElementTypes.ElementType.Air:
                    m_airAmount += amount;
                    m_airAmount = Mathf.Clamp(m_airAmount, 0, maxElementStack);
                    OnElementStackChanged?.Invoke(elementType, m_airAmount);
                    break;
                case ElementTypes.ElementType.Earth:
                    m_earthAmount += amount;
                    OnElementStackChanged?.Invoke(elementType, m_earthAmount);
                    break;
            }
            return;
        }
        
        switch (elementType)
        {
            case ElementTypes.ElementType.Fire:
                m_fireAmount += amount;
                m_fireAmount = Mathf.Clamp(m_fireAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_fireAmount);
                break;
            case ElementTypes.ElementType.Water:
                m_waterAmount += amount;
                m_waterAmount = Mathf.Clamp(m_waterAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_waterAmount);
                break;
            case ElementTypes.ElementType.Air:
                m_airAmount += amount;
                m_airAmount = Mathf.Clamp(m_airAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_airAmount);
                break;
            case ElementTypes.ElementType.Earth:
                m_earthAmount += amount;
                m_earthAmount = Mathf.Clamp(m_earthAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_earthAmount);
                break;
        }
    }

    public void RemoveElement(ElementTypes.ElementType elementType, int amount)
    {
        if (infiniteElementStack)
            return;
        
        switch (elementType)
        {
            case ElementTypes.ElementType.Fire:
                m_fireAmount -= amount;
                m_fireAmount = Mathf.Clamp(m_fireAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_fireAmount);
                break;
            case ElementTypes.ElementType.Water:
                m_waterAmount -= amount;
                m_waterAmount = Mathf.Clamp(m_waterAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_waterAmount);
                break;
            case ElementTypes.ElementType.Air:
                m_airAmount -= amount;
                m_airAmount = Mathf.Clamp(m_airAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_airAmount);
                break;
            case ElementTypes.ElementType.Earth:
                m_earthAmount -= amount;
                m_earthAmount = Mathf.Clamp(m_earthAmount, 0, maxElementStack);
                OnElementStackChanged?.Invoke(elementType, m_earthAmount);
                break;
        }
    }
}
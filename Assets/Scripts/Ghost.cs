using UnityEngine;

public enum GhostType
{
    Poltergeist, // Can move objects, throw things
    Banshee,     // Can wail to cause fear
    Wraith,      // Can appear and disappear, phase through walls
    Phantom,     // Can possess mortals briefly
    Specter      // Can chill areas, freeze objects
}

[RequireComponent(typeof(Collider))]
public class Ghost : MonoBehaviour
{
    [Header("Ghost Properties")]
    public GhostType ghostType = GhostType.Poltergeist;
    public string ghostName = "Unnamed Ghost";
    public int bindCost = 10;
    public int primaryPowerCost = 5;
    public int secondaryPowerCost = 8;
    public float powerRange = 5f;
    public float powerCooldown = 2f;
    
    [Header("Visual")]
    public GameObject selectionIndicator;
    public ParticleSystem ghostEffect;
    public Renderer ghostRenderer;
    
    // State
    private bool isBound = false;
    private bool isSelected = false;
    private Anchor currentAnchor;
    private float lastPowerUse = 0f;
    private Color originalColor;
    
    // Components
    private Collider ghostCollider;
    
    void Awake()
    {
        ghostCollider = GetComponent<Collider>();
        if (ghostRenderer != null)
        {
            originalColor = ghostRenderer.material.color;
        }
    }
    
    public void Initialize()
    {
        // Set up ghost based on type
        SetupGhostType();
        
        // Initially hide selection indicator
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }
        
        // Start ghost effect if available
        if (ghostEffect != null)
        {
            ghostEffect.Play();
        }
        
        Debug.Log($"Ghost {ghostName} ({ghostType}) initialized");
    }
    
    void SetupGhostType()
    {
        switch (ghostType)
        {
            case GhostType.Poltergeist:
                powerRange = 4f;
                primaryPowerCost = 3;
                secondaryPowerCost = 6;
                break;
            case GhostType.Banshee:
                powerRange = 8f;
                primaryPowerCost = 5;
                secondaryPowerCost = 10;
                break;
            case GhostType.Wraith:
                powerRange = 3f;
                primaryPowerCost = 4;
                secondaryPowerCost = 7;
                break;
            case GhostType.Phantom:
                powerRange = 2f;
                primaryPowerCost = 8;
                secondaryPowerCost = 12;
                break;
            case GhostType.Specter:
                powerRange = 6f;
                primaryPowerCost = 4;
                secondaryPowerCost = 8;
                break;
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
        
        // Change ghost color when selected
        if (ghostRenderer != null)
        {
            ghostRenderer.material.color = selected ? Color.yellow : originalColor;
        }
    }
    
    public bool TryBindToAnchor(Anchor anchor)
    {
        if (isBound || anchor.IsOccupied())
        {
            Debug.Log($"Cannot bind {ghostName} - already bound or anchor occupied");
            return false;
        }
        
        if (!GameManager.Instance.SpendPlasm(bindCost))
        {
            Debug.Log($"Not enough plasm to bind {ghostName}");
            return false;
        }
        
        // Move ghost to anchor position
        transform.position = anchor.transform.position;
        
        // Bind to anchor
        isBound = true;
        currentAnchor = anchor;
        anchor.BindGhost(this);
        
        Debug.Log($"{ghostName} bound to anchor at {anchor.transform.position}");
        return true;
    }
    
    public void Unbind()
    {
        if (isBound && currentAnchor != null)
        {
            currentAnchor.UnbindGhost();
            currentAnchor = null;
            isBound = false;
            Debug.Log($"{ghostName} unbound from anchor");
        }
    }
    
    public void ActivatePrimaryPower()
    {
        if (!CanUsePower(primaryPowerCost))
        {
            return;
        }
        
        GameManager.Instance.SpendPlasm(primaryPowerCost);
        lastPowerUse = Time.time;
        
        switch (ghostType)
        {
            case GhostType.Poltergeist:
                PoltergeistPrimary();
                break;
            case GhostType.Banshee:
                BansheePrimary();
                break;
            case GhostType.Wraith:
                WraithPrimary();
                break;
            case GhostType.Phantom:
                PhantomPrimary();
                break;
            case GhostType.Specter:
                SpecterPrimary();
                break;
        }
        
        Debug.Log($"{ghostName} used primary power");
    }
    
    public void ActivateSecondaryPower()
    {
        if (!CanUsePower(secondaryPowerCost))
        {
            return;
        }
        
        GameManager.Instance.SpendPlasm(secondaryPowerCost);
        lastPowerUse = Time.time;
        
        switch (ghostType)
        {
            case GhostType.Poltergeist:
                PoltergeistSecondary();
                break;
            case GhostType.Banshee:
                BansheeSecondary();
                break;
            case GhostType.Wraith:
                WraithSecondary();
                break;
            case GhostType.Phantom:
                PhantomSecondary();
                break;
            case GhostType.Specter:
                SpecterSecondary();
                break;
        }
        
        Debug.Log($"{ghostName} used secondary power");
    }
    
    bool CanUsePower(int cost)
    {
        if (!isBound)
        {
            Debug.Log($"{ghostName} must be bound to an anchor to use powers");
            return false;
        }
        
        if (Time.time - lastPowerUse < powerCooldown)
        {
            Debug.Log($"{ghostName} power on cooldown");
            return false;
        }
        
        if (GameManager.Instance.GetCurrentPlasm() < cost)
        {
            Debug.Log($"Not enough plasm for {ghostName} power");
            return false;
        }
        
        return true;
    }
    
    // Ghost Type Specific Powers
    void PoltergeistPrimary() // Throw Object
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(20f);
            // TODO: Add object throwing visual effect
        }
    }
    
    void PoltergeistSecondary() // Shake Room
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange * 1.5f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(35f);
        }
        // TODO: Add room shaking effect
    }
    
    void BansheePrimary() // Wail
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(30f);
        }
        // TODO: Add wailing sound effect
    }
    
    void BansheeSecondary() // Terrifying Scream
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(50f);
            mortal.Stun(2f);
        }
    }
    
    void WraithPrimary() // Phase
    {
        // TODO: Implement phasing mechanics
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(25f);
        }
    }
    
    void WraithSecondary() // Possession Attempt
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        if (mortals.Count > 0)
        {
            mortals[0].TakeFear(40f);
            // TODO: Add possession effect
        }
    }
    
    void PhantomPrimary() // Appear
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(35f);
        }
    }
    
    void PhantomSecondary() // Possess
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        if (mortals.Count > 0)
        {
            mortals[0].Possess(3f);
        }
    }
    
    void SpecterPrimary() // Chill
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(20f);
            mortal.Slow(0.5f, 3f);
        }
    }
    
    void SpecterSecondary() // Freeze
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(30f);
            mortal.Freeze(2f);
        }
    }
    
    // Getters
    public bool IsBound() => isBound;
    public bool IsSelected() => isSelected;
    public GhostType GetGhostType() => ghostType;
    public string GetGhostName() => ghostName;
    public Anchor GetCurrentAnchor() => currentAnchor;
    public float GetPowerRange() => powerRange;
}
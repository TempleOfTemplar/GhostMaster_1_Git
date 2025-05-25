using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Plasm System")]
    public float maxPlasm = 100f;
    public float currentPlasm = 50f;
    public float plasmRegenRate = 2f; // per second
    
    [Header("Visual")]
    public GameObject selectionIndicator;
    public ParticleSystem ghostEffect;
    public Renderer ghostRenderer;
    
    [Header("Abilities")]
    public List<GhostAbility> abilities = new List<GhostAbility>();
    
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
    
    void Start()
    {
        Initialize();
    }
    
    void Update()
    {
        // Regenerate plasm over time
        RegeneratePlasm();
    }
    
    void RegeneratePlasm()
    {
        if (currentPlasm < maxPlasm)
        {
            currentPlasm = Mathf.Min(maxPlasm, currentPlasm + plasmRegenRate * Time.deltaTime);
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
        
        // Initialize abilities based on ghost type
        InitializeAbilities();
        
        Debug.Log($"Ghost {ghostName} ({ghostType}) initialized");
    }
    
    void InitializeAbilities()
    {
        abilities.Clear();
        
        switch (ghostType)
        {
            case GhostType.Poltergeist:
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Throw Object", 
                    description = "Throw nearby objects at mortals",
                    plasmCost = primaryPowerCost, 
                    cooldownTime = powerCooldown 
                });
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Shake Room", 
                    description = "Violently shake the entire room",
                    plasmCost = secondaryPowerCost, 
                    cooldownTime = powerCooldown * 1.5f 
                });
                break;
                
            case GhostType.Banshee:
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Wail", 
                    description = "Let out a terrifying wail",
                    plasmCost = primaryPowerCost, 
                    cooldownTime = powerCooldown 
                });
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Terrifying Scream", 
                    description = "Paralyze mortals with terror",
                    plasmCost = secondaryPowerCost, 
                    cooldownTime = powerCooldown * 2f 
                });
                break;
                
            case GhostType.Wraith:
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Phase", 
                    description = "Phase through walls and appear suddenly",
                    plasmCost = primaryPowerCost, 
                    cooldownTime = powerCooldown 
                });
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Possession Attempt", 
                    description = "Try to possess a mortal",
                    plasmCost = secondaryPowerCost, 
                    cooldownTime = powerCooldown * 1.8f 
                });
                break;
                
            case GhostType.Phantom:
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Appear", 
                    description = "Suddenly manifest to scare mortals",
                    plasmCost = primaryPowerCost, 
                    cooldownTime = powerCooldown 
                });
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Possess", 
                    description = "Take control of a mortal",
                    plasmCost = secondaryPowerCost, 
                    cooldownTime = powerCooldown * 2.5f 
                });
                break;
                
            case GhostType.Specter:
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Chill", 
                    description = "Lower temperature and slow mortals",
                    plasmCost = primaryPowerCost, 
                    cooldownTime = powerCooldown 
                });
                abilities.Add(new GhostAbility 
                { 
                    abilityName = "Freeze", 
                    description = "Freeze mortals in place",
                    plasmCost = secondaryPowerCost, 
                    cooldownTime = powerCooldown * 1.7f 
                });
                break;
        }
    }
    
    void SetupGhostType()
    {
        switch (ghostType)
        {
            case GhostType.Poltergeist:
                powerRange = 4f;
                primaryPowerCost = 3;
                secondaryPowerCost = 6;
                maxPlasm = 80f;
                plasmRegenRate = 2.5f;
                break;
            case GhostType.Banshee:
                powerRange = 8f;
                primaryPowerCost = 5;
                secondaryPowerCost = 10;
                maxPlasm = 100f;
                plasmRegenRate = 2f;
                break;
            case GhostType.Wraith:
                powerRange = 3f;
                primaryPowerCost = 4;
                secondaryPowerCost = 7;
                maxPlasm = 90f;
                plasmRegenRate = 3f;
                break;
            case GhostType.Phantom:
                powerRange = 2f;
                primaryPowerCost = 8;
                secondaryPowerCost = 12;
                maxPlasm = 120f;
                plasmRegenRate = 1.5f;
                break;
            case GhostType.Specter:
                powerRange = 6f;
                primaryPowerCost = 4;
                secondaryPowerCost = 8;
                maxPlasm = 85f;
                plasmRegenRate = 2.2f;
                break;
        }
        
        // Set current plasm to half of max initially
        currentPlasm = maxPlasm * 0.5f;
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
        
        if (!HasEnoughPlasm(bindCost))
        {
            Debug.Log($"Not enough plasm to bind {ghostName}");
            return false;
        }
        
        // Consume plasm for binding
        ConsumePlasm(bindCost);
        
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
        
        ConsumePlasm(primaryPowerCost);
        lastPowerUse = Time.time;
        
        // Use first ability if available
        if (abilities.Count > 0)
        {
            abilities[0].Use();
        }
        
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
        
        ConsumePlasm(secondaryPowerCost);
        lastPowerUse = Time.time;
        
        // Use second ability if available
        if (abilities.Count > 1)
        {
            abilities[1].Use();
        }
        
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
        
        if (!HasEnoughPlasm(cost))
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
        
        // Trigger poltergeist effect on nearby interactable objects
        ActivateNearbyPoltergeistEffects();
    }
    
    void PoltergeistSecondary() // Shake Room
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange * 1.5f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(35f);
        }
        // TODO: Add room shaking effect
        
        // Activate all poltergeist effects in extended range
        ActivateNearbyPoltergeistEffects(powerRange * 1.5f);
    }
    
    void ActivateNearbyPoltergeistEffects(float range = -1f)
    {
        if (range < 0) range = powerRange;
        
        Collider[] objects = Physics.OverlapSphere(transform.position, range);
        foreach (Collider obj in objects)
        {
            PoltergeistEffect poltergeist = obj.GetComponent<PoltergeistEffect>();
            if (poltergeist != null)
            {
                poltergeist.Initialize(this, null);
                poltergeist.Activate();
            }
        }
    }
    
    void BansheePrimary() // Wail
    {
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, powerRange);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(30f);
        }
        
        // Play wailing sound effect
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHauntingSound();
        }
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
    
    // MISSING METHODS - PLASM SYSTEM
    public float GetPlasm()
    {
        return currentPlasm;
    }
    
    public float GetMaxPlasm()
    {
        return maxPlasm;
    }
    
    public bool HasEnoughPlasm(float amount)
    {
        return currentPlasm >= amount;
    }
    
    public void ConsumePlasm(float amount)
    {
        currentPlasm = Mathf.Max(0f, currentPlasm - amount);
        Debug.Log($"{ghostName} consumed {amount} plasm. Remaining: {currentPlasm:F1}");
    }
    
    public void AddPlasm(float amount)
    {
        currentPlasm = Mathf.Min(maxPlasm, currentPlasm + amount);
        Debug.Log($"{ghostName} gained {amount} plasm. Current: {currentPlasm:F1}");
    }
    
    public void SetPlasm(float amount)
    {
        currentPlasm = Mathf.Clamp(amount, 0f, maxPlasm);
    }
    
    public float GetPlasmPercentage()
    {
        return maxPlasm > 0f ? currentPlasm / maxPlasm : 0f;
    }
    
    // ADDITIONAL UTILITY METHODS
    public bool CanAfford(float cost)
    {
        return HasEnoughPlasm(cost);
    }
    
    public void RestorePlasm(float amount)
    {
        AddPlasm(amount);
    }
    
    public void DrainPlasm(float amount)
    {
        ConsumePlasm(amount);
    }
    
    public bool IsFullPlasm()
    {
        return currentPlasm >= maxPlasm;
    }
    
    public bool IsLowPlasm(float threshold = 0.25f)
    {
        return GetPlasmPercentage() <= threshold;
    }
    
    // ABILITY SYSTEM INTEGRATION
    public List<GhostAbility> GetAbilities()
    {
        return abilities;
    }
    
    public bool CanUseAbility(int abilityIndex)
    {
        if (abilityIndex < 0 || abilityIndex >= abilities.Count) return false;
        
        GhostAbility ability = abilities[abilityIndex];
        return isBound && 
               HasEnoughPlasm(ability.plasmCost) && 
               ability.CanUse();
    }
    
    public void UseAbility(int abilityIndex)
    {
        if (!CanUseAbility(abilityIndex)) return;
        
        GhostAbility ability = abilities[abilityIndex];
        ConsumePlasm(ability.plasmCost);
        ability.Use();
        
        // Trigger appropriate power based on ability index
        if (abilityIndex == 0)
            ActivatePrimaryPower();
        else if (abilityIndex == 1)
            ActivateSecondaryPower();
    }
    
    // EVENT INTEGRATION
    private void OnDestroy()
    {
        // Clean up when ghost is destroyed
        Unbind();
    }
    
    // Getters (existing)
    public bool IsBound() => isBound;
    public bool IsSelected() => isSelected;
    public GhostType GetGhostType() => ghostType;
    public string GetGhostName() => ghostName;
    public Anchor GetCurrentAnchor() => currentAnchor;
    public float GetPowerRange() => powerRange;
    
    // Additional getters for UI and game state
    public int GetBindCost() => bindCost;
    public int GetPrimaryPowerCost() => primaryPowerCost;
    public int GetSecondaryPowerCost() => secondaryPowerCost;
    public float GetPowerCooldown() => powerCooldown;
    public float GetLastPowerUse() => lastPowerUse;
    
    public bool IsPowerOnCooldown()
    {
        return Time.time - lastPowerUse < powerCooldown;
    }
    
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, powerCooldown - (Time.time - lastPowerUse));
    }
}
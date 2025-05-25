using UnityEngine;

public enum AnchorType
{
    Mirror,        // Good for reflection-based scares
    Electrical,    // Good for electrical disturbances
    Furniture,     // Good for moving objects
    Plumbing,      // Good for water-based scares
    Temperature,   // Good for temperature effects
    Generic        // General purpose anchor
}

public class Anchor : MonoBehaviour
{
    [Header("Anchor Properties")]
    public AnchorType anchorType = AnchorType.Generic;
    public string anchorName = "Unnamed Anchor";
    public bool isOccupied = false;
    public float powerBonus = 1.0f; // Multiplier for ghost powers when used from this anchor
    
    [Header("Visual")]
    public GameObject anchorVisual;
    public GameObject occupiedIndicator;
    public ParticleSystem anchorEffect;
    public Renderer anchorRenderer;
    
    [Header("Restrictions")]
    public GhostType[] compatibleGhostTypes; // Empty means all types can use it
    public bool requiresSpecificGhost = false;
    
    // State
    private Ghost boundGhost;
    private Color originalColor;
    private bool isHighlighted = false;
    
    void Awake()
    {
        if (anchorRenderer != null)
        {
            originalColor = anchorRenderer.material.color;
        }
    }
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // Set up anchor based on type
        SetupAnchorType();
        
        // Initially hide occupied indicator
        if (occupiedIndicator != null)
        {
            occupiedIndicator.SetActive(false);
        }
        
        // Start anchor effect if available
        if (anchorEffect != null && !isOccupied)
        {
            anchorEffect.Play();
        }
        
        Debug.Log($"Anchor {anchorName} ({anchorType}) initialized");
    }
    
    void SetupAnchorType()
    {
        switch (anchorType)
        {
            case AnchorType.Mirror:
                powerBonus = 1.3f;
                compatibleGhostTypes = new GhostType[] { GhostType.Wraith, GhostType.Phantom };
                break;
            case AnchorType.Electrical:
                powerBonus = 1.4f;
                compatibleGhostTypes = new GhostType[] { GhostType.Poltergeist, GhostType.Specter };
                break;
            case AnchorType.Furniture:
                powerBonus = 1.5f;
                compatibleGhostTypes = new GhostType[] { GhostType.Poltergeist };
                break;
            case AnchorType.Plumbing:
                powerBonus = 1.2f;
                compatibleGhostTypes = new GhostType[] { GhostType.Specter, GhostType.Banshee };
                break;
            case AnchorType.Temperature:
                powerBonus = 1.4f;
                compatibleGhostTypes = new GhostType[] { GhostType.Specter };
                break;
            case AnchorType.Generic:
                powerBonus = 1.0f;
                compatibleGhostTypes = new GhostType[0]; // All types compatible
                break;
        }
    }
    
    public bool CanBindGhost(Ghost ghost)
    {
        if (isOccupied)
        {
            return false;
        }
        
        // Check if ghost type is compatible
        if (requiresSpecificGhost && compatibleGhostTypes.Length > 0)
        {
            bool isCompatible = false;
            foreach (GhostType compatibleType in compatibleGhostTypes)
            {
                if (ghost.GetGhostType() == compatibleType)
                {
                    isCompatible = true;
                    break;
                }
            }
            
            if (!isCompatible)
            {
                Debug.Log($"Ghost {ghost.GetGhostName()} is not compatible with anchor {anchorName}");
                return false;
            }
        }
        
        return true;
    }
    
    public bool BindGhost(Ghost ghost)
    {
        if (!CanBindGhost(ghost))
        {
            return false;
        }
        
        boundGhost = ghost;
        isOccupied = true;
        
        // Update visuals
        UpdateVisuals();
        
        // Stop anchor effect when occupied
        if (anchorEffect != null)
        {
            anchorEffect.Stop();
        }
        
        Debug.Log($"Ghost {ghost.GetGhostName()} bound to anchor {anchorName}");
        return true;
    }
    
    public void UnbindGhost()
    {
        if (boundGhost != null)
        {
            Debug.Log($"Ghost {boundGhost.GetGhostName()} unbound from anchor {anchorName}");
            boundGhost = null;
        }
        
        isOccupied = false;
        
        // Update visuals
        UpdateVisuals();
        
        // Restart anchor effect
        if (anchorEffect != null)
        {
            anchorEffect.Play();
        }
    }
    
    void UpdateVisuals()
    {
        // Show/hide occupied indicator
        if (occupiedIndicator != null)
        {
            occupiedIndicator.SetActive(isOccupied);
        }
        
        // Change anchor color when occupied
        if (anchorRenderer != null)
        {
            if (isOccupied)
            {
                anchorRenderer.material.color = Color.green;
            }
            else if (isHighlighted)
            {
                anchorRenderer.material.color = Color.yellow;
            }
            else
            {
                anchorRenderer.material.color = originalColor;
            }
        }
    }
    
    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        UpdateVisuals();
    }
    
    void OnMouseEnter()
    {
        // Highlight anchor when mouse is over it (if player has a ghost selected)
        if (GameManager.Instance != null && GameManager.Instance.GetSelectedGhost() != null && !isOccupied)
        {
            SetHighlighted(true);
        }
    }
    
    void OnMouseExit()
    {
        SetHighlighted(false);
    }
    
    void OnMouseDown()
    {
        // Handle click on anchor
        if (GameManager.Instance != null)
        {
            Ghost selectedGhost = GameManager.Instance.GetSelectedGhost();
            if (selectedGhost != null && !isOccupied)
            {
                selectedGhost.TryBindToAnchor(this);
            }
            else if (isOccupied && boundGhost != null)
            {
                // Select the bound ghost
                GameManager.Instance.SelectGhost(boundGhost);
            }
        }
    }
    
    // Special anchor abilities that can be triggered
    public void TriggerAnchorAbility()
    {
        if (!isOccupied || boundGhost == null) return;
        
        switch (anchorType)
        {
            case AnchorType.Mirror:
                TriggerMirrorEffect();
                break;
            case AnchorType.Electrical:
                TriggerElectricalEffect();
                break;
            case AnchorType.Furniture:
                TriggerFurnitureEffect();
                break;
            case AnchorType.Plumbing:
                TriggerPlumbingEffect();
                break;
            case AnchorType.Temperature:
                TriggerTemperatureEffect();
                break;
        }
    }
    
    void TriggerMirrorEffect()
    {
        // Mirror shatters, scares nearby mortals
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, 8f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(25f);
        }
        // TODO: Add mirror shatter effect
        Debug.Log($"Mirror anchor {anchorName} triggered!");
    }
    
    void TriggerElectricalEffect()
    {
        // Lights flicker, electrical disturbance
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, 10f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(20f);
        }
        // TODO: Add electrical effect
        Debug.Log($"Electrical anchor {anchorName} triggered!");
    }
    
    void TriggerFurnitureEffect()
    {
        // Furniture moves by itself
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, 6f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(30f);
        }
        // TODO: Add furniture movement effect
        Debug.Log($"Furniture anchor {anchorName} triggered!");
    }
    
    void TriggerPlumbingEffect()
    {
        // Water pipes make noise, leak, or spray
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, 7f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(15f);
        }
        // TODO: Add plumbing effect
        Debug.Log($"Plumbing anchor {anchorName} triggered!");
    }
    
    void TriggerTemperatureEffect()
    {
        // Room gets cold suddenly
        var mortals = GameManager.Instance.GetMortalsInRange(transform.position, 12f);
        foreach (var mortal in mortals)
        {
            mortal.TakeFear(18f);
            mortal.Slow(0.8f, 3f);
        }
        // TODO: Add temperature effect
        Debug.Log($"Temperature anchor {anchorName} triggered!");
    }
    
    // Public getters
    public bool IsOccupied() => isOccupied;
    public Ghost GetBoundGhost() => boundGhost;
    public AnchorType GetAnchorType() => anchorType;
    public string GetAnchorName() => anchorName;
    public float GetPowerBonus() => powerBonus;
    public GhostType[] GetCompatibleGhostTypes() => compatibleGhostTypes;
}
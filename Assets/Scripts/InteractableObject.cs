// InteractableObject.cs - Objects that can be haunted
using UnityEngine;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string objectName = "Object";
    public bool canBeHaunted = true;
    public List<HauntingEffect> availableEffects = new List<HauntingEffect>();
    public float interactionCooldown = 2f;
    
    [Header("Visual Feedback")]
    public GameObject highlightEffect;
    public Color highlightColor = Color.cyan;
    
    private float lastInteractionTime;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;
            
        // Initialize haunting effects
        foreach (var effect in availableEffects)
        {
            if (effect != null)
                effect.Initialize(null, null);
        }
    }
    
    public bool CanInteract()
    {
        return canBeHaunted && Time.time > lastInteractionTime + interactionCooldown;
    }
    
    public void Interact(Ghost ghost)
    {
        if (!CanInteract()) return;
        
        lastInteractionTime = Time.time;
        
        // Activate the first available effect
        foreach (var effect in availableEffects)
        {
            if (effect != null && effect.CanActivate())
            {
                effect.Initialize(ghost, null);
                effect.Activate();
                break;
            }
        }
    }
    
    public void Highlight(bool highlight)
    {
        isHighlighted = highlight;
        
        if (highlightEffect != null)
            highlightEffect.SetActive(highlight);
            
        if (objectRenderer != null)
        {
            objectRenderer.material.color = highlight ? highlightColor : originalColor;
        }
    }
    
    public List<HauntingEffect> GetAvailableEffects()
    {
        return availableEffects;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
// HauntingEffect.cs - Base class for all haunting effects
using UnityEngine;
using System.Collections;

public abstract class HauntingEffect : MonoBehaviour
{
    [Header("Haunting Settings")]
    public float duration = 5f;
    public float intensity = 1f;
    public float plasm_cost = 10f;
    public AudioClip effectSound;
    
    protected bool isActive = false;
    protected Ghost assignedGhost;
    protected Mortal targetMortal;
    
    public virtual void Initialize(Ghost ghost, Mortal target)
    {
        assignedGhost = ghost;
        targetMortal = target;
    }
    
    public virtual bool CanActivate()
    {
        return !isActive && assignedGhost != null && assignedGhost.GetPlasm() >= plasm_cost;
    }
    
    public virtual void Activate()
    {
        if (!CanActivate()) return;
        
        isActive = true;
        assignedGhost.ConsumePlasm(plasm_cost);
        
        if (effectSound != null)
            AudioSource.PlayClipAtPoint(effectSound, transform.position);
            
        StartCoroutine(EffectCoroutine());
    }
    
    protected virtual IEnumerator EffectCoroutine()
    {
        OnEffectStart();
        yield return new WaitForSeconds(duration);
        OnEffectEnd();
        isActive = false;
    }
    
    protected abstract void OnEffectStart();
    protected abstract void OnEffectEnd();
    
    public virtual void Deactivate()
    {
        if (isActive)
        {
            StopAllCoroutines();
            OnEffectEnd();
            isActive = false;
        }
    }
}

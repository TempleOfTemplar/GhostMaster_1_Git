// ScareEffect.cs - Makes mortals run away in fear
using UnityEngine;

public class ScareEffect : HauntingEffect
{
    [Header("Scare Settings")]
    public float scareRadius = 5f;
    public float fearLevel = 50f;
    public ParticleSystem scareParticles;
    
    protected override void OnEffectStart()
    {
        Collider[] mortalsInRange = Physics.OverlapSphere(transform.position, scareRadius);
        
        foreach (Collider col in mortalsInRange)
        {
            Mortal mortal = col.GetComponent<Mortal>();
            if (mortal != null)
            {
                mortal.AddFear(fearLevel * intensity);
                mortal.TriggerFleeState();
            }
        }
        
        if (scareParticles != null)
            scareParticles.Play();
    }
    
    protected override void OnEffectEnd()
    {
        if (scareParticles != null)
            scareParticles.Stop();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scareRadius);
    }
}

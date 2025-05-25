// PoltergeistEffect.cs - Moves objects around
using UnityEngine;
using System.Collections;

public class PoltergeistEffect : HauntingEffect
{
    [Header("Poltergeist Settings")]
    public float throwForce = 500f;
    public float effectRadius = 8f;
    public LayerMask movableObjects = -1;
    
    protected override void OnEffectStart()
    {
        StartCoroutine(ThrowObjectsCoroutine());
    }
    
    private IEnumerator ThrowObjectsCoroutine()
    {
        float timer = 0f;
        while (timer < duration)
        {
            ThrowNearbyObjects();
            yield return new WaitForSeconds(0.5f);
            timer += 0.5f;
        }
    }
    
    private void ThrowNearbyObjects()
    {
        Collider[] objects = Physics.OverlapSphere(transform.position, effectRadius, movableObjects);
        
        foreach (Collider obj in objects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (obj.transform.position - transform.position).normalized;
                direction.y = Random.Range(0.5f, 1f);
                rb.AddForce(direction * throwForce * intensity);
                
                // Scare nearby mortals
                Collider[] nearbyMortals = Physics.OverlapSphere(obj.transform.position, 3f);
                foreach (Collider mortalCol in nearbyMortals)
                {
                    Mortal mortal = mortalCol.GetComponent<Mortal>();
                    if (mortal != null)
                        mortal.AddFear(20f);
                }
            }
        }
    }
    
    protected override void OnEffectEnd()
    {
        // Effect cleanup if needed
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}

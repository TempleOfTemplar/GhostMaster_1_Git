// PlasmCollector.cs - Collects plasm for ghosts
using UnityEngine;

public class PlasmCollector : MonoBehaviour
{
    [Header("Plasm Settings")]
    public float plasmAmount = 25f;
    public float rotationSpeed = 50f;
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    
    private bool collected = false;
    
    private void Update()
    {
        if (!collected)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            CollectPlasm(ghost);
        }
    }
    
    private void CollectPlasm(Ghost ghost)
    {
        collected = true;
        ghost.AddPlasm(plasmAmount);
        
        if (collectEffect != null)
        {
            collectEffect.transform.SetParent(null);
            collectEffect.Play();
            Destroy(collectEffect.gameObject, 3f);
        }
        
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        
        Debug.Log($"Ghost collected {plasmAmount} plasm!");
        
        // Hide the collector
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        Destroy(gameObject, 1f);
    }
}

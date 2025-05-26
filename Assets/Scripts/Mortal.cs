using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum MortalType
{
    Child,      // Easy to scare, runs fast
    Adult,      // Normal fear resistance
    Elderly,    // Slow but hard to scare
    Skeptic,    // Very hard to scare
    Believer    // Easy to scare but may become aggressive
}

[RequireComponent(typeof(NavMeshAgent))]
public class Mortal : MonoBehaviour
{
    [Header("Mortal Properties")]
    public MortalType mortalType = MortalType.Adult;
    public string mortalName = "Unnamed Person";
    public float maxFear = 100f;
    public float fearDecayRate = 5f;
    public float scareThreshold = 80f;
    public float fleeSpeed = 3.5f;
    
    [Header("Visual")]
    public GameObject fearIndicator;
    public Renderer mortalRenderer;
    public Animator mortalAnimator;
    
    [Header("Behavior")]
    public Transform[] waypoints;
    public float wanderRadius = 10f;
    public float waitTime = 2f;
    
    // State
    private float currentFear = 0f;
    private bool isScaredAway = false;
    private bool isActive = true;
    private bool isStunned = false;
    private bool isPossessed = false;
    private bool isFrozen = false;
    private bool hasFled = false;
    private float originalSpeed;
    private int currentWaypoint = 0;
    
    // Status Effects
    private float stunTimer = 0f;
    private float possessionTimer = 0f;
    private float freezeTimer = 0f;
    private float slowTimer = 0f;
    private float slowMultiplier = 1f;
    
    // Components
    private NavMeshAgent agent;
    private Color originalColor;
    
    // Coroutines
    private Coroutine wanderCoroutine;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;
        
        if (mortalRenderer != null)
        {
            originalColor = mortalRenderer.material.color;
        }
    }
    
    public void Initialize()
    {
        SetupMortalType();
        
        if (fearIndicator != null)
        {
            fearIndicator.SetActive(false);
        }
        
        // Start wandering behavior
        if (waypoints.Length > 0)
        {
            wanderCoroutine = StartCoroutine(WanderBehavior());
        }
        else
        {
            wanderCoroutine = StartCoroutine(RandomWanderBehavior());
        }
        
        Debug.Log($"Mortal {mortalName} ({mortalType}) initialized");
    }
    
    void SetupMortalType()
    {
        switch (mortalType)
        {
            case MortalType.Child:
                maxFear = 60f;
                scareThreshold = 40f;
                fleeSpeed = 4f;
                fearDecayRate = 3f;
                break;
            case MortalType.Adult:
                maxFear = 100f;
                scareThreshold = 80f;
                fleeSpeed = 3.5f;
                fearDecayRate = 5f;
                break;
            case MortalType.Elderly:
                maxFear = 120f;
                scareThreshold = 100f;
                fleeSpeed = 2f;
                fearDecayRate = 2f;
                break;
            case MortalType.Skeptic:
                maxFear = 150f;
                scareThreshold = 120f;
                fleeSpeed = 3f;
                fearDecayRate = 8f;
                break;
            case MortalType.Believer:
                maxFear = 80f;
                scareThreshold = 60f;
                fleeSpeed = 4.5f;
                fearDecayRate = 2f;
                break;
        }
        
        agent.speed = fleeSpeed * 0.7f; // Normal walking speed
    }
    
    void Update()
    {
        if (!isActive) return;
        
        // Update status effects
        UpdateStatusEffects();
        
        // Decay fear over time
        if (currentFear > 0 && !isPossessed)
        {
            currentFear = Mathf.Max(0, currentFear - fearDecayRate * Time.deltaTime);
            UpdateFearVisuals();
        }
        
        // Check if scared away
        if (currentFear >= scareThreshold && !isScaredAway)
        {
            BecomeScaredAway();
        }
    }
    
    void UpdateStatusEffects()
    {
        // Update stun
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                agent.isStopped = false;
            }
        }
        
        // Update possession
        if (isPossessed)
        {
            possessionTimer -= Time.deltaTime;
            if (possessionTimer <= 0)
            {
                EndPossession();
            }
        }
        
        // Update freeze
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0)
            {
                isFrozen = false;
                agent.isStopped = false;
                if (mortalRenderer != null)
                {
                    mortalRenderer.material.color = originalColor;
                }
            }
        }
        
        // Update slow
        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                agent.speed = originalSpeed;
                slowMultiplier = 1f;
            }
        }
    }
    
    // Method called by PoltergeistEffect and other scripts
    public void AddFear(float amount)
    {
        TakeFear(amount);
    }
    
    public void TakeFear(float amount)
    {
        if (isScaredAway || !isActive) return;
        
        // Modify fear based on mortal type
        float modifiedAmount = amount;
        switch (mortalType)
        {
            case MortalType.Child:
                modifiedAmount *= 1.5f;
                break;
            case MortalType.Skeptic:
                modifiedAmount *= 0.5f;
                break;
            case MortalType.Believer:
                modifiedAmount *= 1.3f;
                break;
        }
        
        currentFear = Mathf.Min(currentFear + modifiedAmount, maxFear);
        UpdateFearVisuals();
        
        // Interrupt current action
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
        }
        
        // Show fear reaction
        StartCoroutine(FearReaction());
        
        Debug.Log($"{mortalName} fear: {currentFear:F1}/{maxFear}");
    }
    
    // Method called by ScareEffect
    public void TriggerFleeState()
    {
        if (isScaredAway || !isActive) return;
        
        // Force the mortal to become scared away regardless of current fear level
        BecomeScaredAway();
    }
    
    // Method called by MissionManager to get fear level
    public float GetFearLevel()
    {
        return currentFear;
    }
    
    // Method called by MissionManager to check if mortal has fled
    public bool HasFled()
    {
        return hasFled;
    }
    
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        agent.isStopped = true;
        
        if (mortalAnimator != null)
        {
            mortalAnimator.SetTrigger("Stunned");
        }
    }
    
    public void Possess(float duration)
    {
        isPossessed = true;
        possessionTimer = duration;
        
        // Stop normal behavior
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
        }
        
        // Visual effect for possession
        if (mortalRenderer != null)
        {
            mortalRenderer.material.color = Color.red;
        }
        
        StartCoroutine(PossessionBehavior());
    }
    
    void EndPossession()
    {
        isPossessed = false;
        
        if (mortalRenderer != null)
        {
            mortalRenderer.material.color = originalColor;
        }
        
        // Resume normal behavior
        if (!isScaredAway)
        {
            if (waypoints.Length > 0)
            {
                wanderCoroutine = StartCoroutine(WanderBehavior());
            }
            else
            {
                wanderCoroutine = StartCoroutine(RandomWanderBehavior());
            }
        }
    }
    
    public void Freeze(float duration)
    {
        isFrozen = true;
        freezeTimer = duration;
        agent.isStopped = true;
        
        if (mortalRenderer != null)
        {
            mortalRenderer.material.color = Color.cyan;
        }
    }
    
    public void Slow(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        slowTimer = duration;
        agent.speed = originalSpeed * multiplier;
    }
    
    void BecomeScaredAway()
    {
        isScaredAway = true;
        hasFled = true; // Set the fled flag
        agent.speed = fleeSpeed;
        
        // Stop current behavior
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
        }
        
        // Find nearest exit and flee
        GameObject[] exits = GameObject.FindGameObjectsWithTag("Exit");
        if (exits.Length > 0)
        {
            Transform nearestExit = exits[0].transform;
            float nearestDistance = Vector3.Distance(transform.position, nearestExit.position);
            
            foreach (GameObject exit in exits)
            {
                float distance = Vector3.Distance(transform.position, exit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestExit = exit.transform;
                    nearestDistance = distance;
                }
            }
            
            agent.SetDestination(nearestExit.position);
            StartCoroutine(FleeToExit(nearestExit));
        }
        else
        {
            // No exit found, just run to a random far location
            Vector3 fleeDirection = (transform.position - Camera.main.transform.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * 20f;
            agent.SetDestination(fleeTarget);
        }
        
        if (mortalAnimator != null)
        {
            mortalAnimator.SetBool("Fleeing", true);
        }
        
        Debug.Log($"{mortalName} is scared away!");
        
        // Notify game manager to check win condition
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinCondition();
        }
    }
    
    IEnumerator FearReaction()
    {
        if (mortalAnimator != null)
        {
            mortalAnimator.SetTrigger("Scared");
        }
        
        // Stop for a moment
        agent.isStopped = true;
        yield return new WaitForSeconds(1f);
        
        if (!isStunned && !isFrozen && !isScaredAway)
        {
            agent.isStopped = false;
            
            // Resume wandering
            if (waypoints.Length > 0)
            {
                wanderCoroutine = StartCoroutine(WanderBehavior());
            }
            else
            {
                wanderCoroutine = StartCoroutine(RandomWanderBehavior());
            }
        }
    }
    
    IEnumerator WanderBehavior()
    {
        while (!isScaredAway && isActive && !isPossessed)
        {
            if (isStunned || isFrozen)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            
            Transform targetWaypoint = waypoints[currentWaypoint];
            agent.SetDestination(targetWaypoint.position);
            
            // Wait until we reach the waypoint
            while (agent.pathPending || agent.remainingDistance > 0.5f)
            {
                if (isStunned || isFrozen || isPossessed || isScaredAway)
                    yield break;
                yield return new WaitForSeconds(0.1f);
            }
            
            // Wait at waypoint
            yield return new WaitForSeconds(waitTime);
            
            // Move to next waypoint
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
    
    IEnumerator RandomWanderBehavior()
    {
        while (!isScaredAway && isActive && !isPossessed)
        {
            if (isStunned || isFrozen)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            
            // Pick a random point within wander radius
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
            {
                agent.SetDestination(hit.position);
                
                // Wait until we reach the destination
                while (agent.pathPending || agent.remainingDistance > 0.5f)
                {
                    if (isStunned || isFrozen || isPossessed || isScaredAway)
                        yield break;
                    yield return new WaitForSeconds(0.1f);
                }
                
                // Wait at destination
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
    
    IEnumerator PossessionBehavior()
    {
        while (isPossessed)
        {
            // During possession, mortal acts erratically
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 5f, 1))
            {
                agent.SetDestination(hit.position);
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    IEnumerator FleeToExit(Transform exit)
    {
        while (Vector3.Distance(transform.position, exit.position) > 2f && isActive)
        {
            agent.SetDestination(exit.position);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Reached exit, remove from game
        isActive = false;
        gameObject.SetActive(false);
        Debug.Log($"{mortalName} has fled the building!");
        
        // Give player some plasm for scaring away mortal
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GainPlasm(10);
        }
    }
    
    void UpdateFearVisuals()
    {
        if (fearIndicator != null)
        {
            fearIndicator.SetActive(currentFear > 10f);
            
            // Scale fear indicator based on fear level
            float fearRatio = currentFear / maxFear;
            fearIndicator.transform.localScale = Vector3.one * (0.5f + fearRatio * 0.5f);
        }
        
        // Change mortal color based on fear level
        if (mortalRenderer != null && !isPossessed && !isFrozen)
        {
            float fearRatio = currentFear / maxFear;
            Color fearColor = Color.Lerp(originalColor, Color.red, fearRatio * 0.3f);
            mortalRenderer.material.color = fearColor;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check for special interactions
        if (other.CompareTag("ScaryObject"))
        {
            TakeFear(15f);
        }
    }
    
    // Public getters - existing methods
    public bool IsScaredAway() => isScaredAway;
    public bool IsActive() => isActive;
    public float GetCurrentFear() => currentFear;
    public float GetFearRatio() => currentFear / maxFear;
    public MortalType GetMortalType() => mortalType;
    public string GetMortalName() => mortalName;
    public bool IsStunned() => isStunned;
    public bool IsPossessed() => isPossessed;
    public bool IsFrozen() => isFrozen;
}
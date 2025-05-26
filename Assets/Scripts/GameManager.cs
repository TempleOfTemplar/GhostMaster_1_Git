using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int maxPlasm = 100;
    public int startingPlasm = 50;
    public float timeScale = 1f;
    
    [Header("UI References")]
    public GameObject ghostSelectionPanel;
    public GameObject plasmMeter;
    
    // Game State
    private int currentPlasm;
    private List<Ghost> availableGhosts = new List<Ghost>();
    private List<Mortal> mortals = new List<Mortal>();
    private List<Anchor> anchors = new List<Anchor>();
    private Ghost selectedGhost;
    private bool gameActive = true;
    private bool gamePaused = false;
    
    // Events
    public System.Action<int> OnPlasmChanged;
    public System.Action<Ghost> OnGhostSelected;
    public System.Action OnLevelComplete;
    public System.Action<int> OnMissionCompleted;
    public System.Action<string> OnMissionFailed;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        currentPlasm = startingPlasm;
        OnPlasmChanged?.Invoke(currentPlasm);
        
        // Find all game objects in scene
        FindAllGhosts();
        FindAllMortals();
        FindAllAnchors();
        
        Debug.Log($"Game initialized with {availableGhosts.Count} ghosts, {mortals.Count} mortals, {anchors.Count} anchors");
    }
    
    void FindAllGhosts()
    {
        Ghost[] ghosts = FindObjectsOfType<Ghost>();
        availableGhosts.Clear();
        availableGhosts.AddRange(ghosts);
        
        foreach (Ghost ghost in ghosts)
        {
            ghost.Initialize();
        }
    }
    
    void FindAllMortals()
    {
        Mortal[] foundMortals = FindObjectsOfType<Mortal>();
        mortals.Clear();
        mortals.AddRange(foundMortals);
        
        foreach (Mortal mortal in foundMortals)
        {
            mortal.Initialize();
        }
    }
    
    void FindAllAnchors()
    {
        Anchor[] foundAnchors = FindObjectsOfType<Anchor>();
        anchors.Clear();
        anchors.AddRange(foundAnchors);
    }
    
    public bool SpendPlasm(int amount)
    {
        if (currentPlasm >= amount)
        {
            currentPlasm -= amount;
            OnPlasmChanged?.Invoke(currentPlasm);
            return true;
        }
        return false;
    }
    
    public void GainPlasm(int amount)
    {
        currentPlasm = Mathf.Min(currentPlasm + amount, maxPlasm);
        OnPlasmChanged?.Invoke(currentPlasm);
    }
    
    public void SelectGhost(Ghost ghost)
    {
        if (selectedGhost != null)
        {
            selectedGhost.SetSelected(false);
        }
        
        selectedGhost = ghost;
        if (ghost != null)
        {
            ghost.SetSelected(true);
        }
        
        OnGhostSelected?.Invoke(ghost);
    }
    
    public Ghost GetSelectedGhost()
    {
        return selectedGhost;
    }
    
    public List<Mortal> GetMortalsInRange(Vector3 position, float range)
    {
        List<Mortal> mortalsInRange = new List<Mortal>();
        
        foreach (Mortal mortal in mortals)
        {
            if (Vector3.Distance(position, mortal.transform.position) <= range)
            {
                mortalsInRange.Add(mortal);
            }
        }
        
        return mortalsInRange;
    }
    
    public List<Anchor> GetAnchorsInRange(Vector3 position, float range)
    {
        List<Anchor> anchorsInRange = new List<Anchor>();
        
        foreach (Anchor anchor in anchors)
        {
            if (Vector3.Distance(position, anchor.transform.position) <= range)
            {
                anchorsInRange.Add(anchor);
            }
        }
        
        return anchorsInRange;
    }
    
    public void CheckWinCondition()
    {
        // Check if all mortals have been scared away or objective completed
        bool allMortalsScared = true;
        foreach (Mortal mortal in mortals)
        {
            if (mortal.IsActive() && !mortal.IsScaredAway())
            {
                allMortalsScared = false;
                break;
            }
        }
        
        if (allMortalsScared)
        {
            OnLevelComplete?.Invoke();
            Debug.Log("Level Complete!");
        }
    }
    
    // Missing methods that were called from MissionManager
    public void OnMissionCompleted(int score)
    {
        gameActive = false;
        Debug.Log($"Mission completed with score: {score}");
        OnMissionCompleted?.Invoke(score);
        
        // You can add additional logic here like:
        // - Save high score
        // - Unlock next level
        // - Award achievements
    }
    
    public void OnMissionFailed(string reason)
    {
        gameActive = false;
        Debug.Log($"Mission failed: {reason}");
        OnMissionFailed?.Invoke(reason);
        
        // You can add additional logic here like:
        // - Show retry options
        // - Reset level state
        // - Track failure statistics
    }
    
    // Game state management
    public void PauseGame()
    {
        gamePaused = true;
        timeScale = 0f;
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        gamePaused = false;
        timeScale = 1f;
        Time.timeScale = 1f;
    }
    
    public bool IsGamePaused()
    {
        return gamePaused;
    }
    
    public bool IsGameActive()
    {
        return gameActive;
    }
    
    public void SetGameActive(bool active)
    {
        gameActive = active;
    }
    
    // Utility methods
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void LoadNextLevel()
    {
        int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;
        
        if (nextScene < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("No more levels available!");
        }
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        // Handle input
        HandleInput();
        
        // Update time scale
        if (!gamePaused)
        {
            Time.timeScale = timeScale;
        }
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
        
        // Hotkeys for ghost powers
        if (Input.GetKeyDown(KeyCode.Space) && selectedGhost != null)
        {
            selectedGhost.ActivatePrimaryPower();
        }
        
        if (Input.GetKeyDown(KeyCode.Q) && selectedGhost != null)
        {
            selectedGhost.ActivateSecondaryPower();
        }
        
        // Pause/Resume with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Check if clicked on ghost
            Ghost clickedGhost = hit.collider.GetComponent<Ghost>();
            if (clickedGhost != null)
            {
                SelectGhost(clickedGhost);
                return;
            }
            
            // Check if clicked on anchor and we have a selected ghost
            Anchor clickedAnchor = hit.collider.GetComponent<Anchor>();
            if (clickedAnchor != null && selectedGhost != null)
            {
                selectedGhost.TryBindToAnchor(clickedAnchor);
                return;
            }
            
            // If clicked on empty space, deselect ghost
            SelectGhost(null);
        }
    }
    
    // Getter methods
    public int GetCurrentPlasm() => currentPlasm;
    public List<Ghost> GetAvailableGhosts() => availableGhosts;
    public List<Mortal> GetMortals() => mortals;
    public List<Anchor> GetAnchors() => anchors;
}
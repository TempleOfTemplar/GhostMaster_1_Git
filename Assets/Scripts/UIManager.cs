using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject ghostSelectionPanel;
    public GameObject ghostInfoPanel;
    public GameObject mortalInfoPanel;
    public GameObject gameOverPanel;
    public GameObject missionCompletePanel;
    public GameObject missionFailedPanel;
    
    [Header("Plasm UI")]
    public Slider plasmSlider;
    public TextMeshProUGUI plasmText;
    public Image plasmFillImage;
    
    [Header("Ghost Selection")]
    public Transform ghostButtonContainer;
    public GameObject ghostButtonPrefab;
    public TextMeshProUGUI selectedGhostNameText;
    public TextMeshProUGUI selectedGhostTypeText;
    public Button primaryPowerButton;
    public Button secondaryPowerButton;
    public TextMeshProUGUI primaryPowerCostText;
    public TextMeshProUGUI secondaryPowerCostText;
    
    [Header("Mortal Status")]
    public Transform mortalStatusContainer;
    public GameObject mortalStatusPrefab;
    
    [Header("Game Info")]
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI timeText;
    public Button pauseButton;
    public Slider timeScaleSlider;
    
    [Header("Messages")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 3f;
    
    [Header("Mission Results")]
    public TextMeshProUGUI missionCompleteScoreText;
    public TextMeshProUGUI missionFailedReasonText;
    public Button restartButton;
    public Button nextLevelButton;
    public Button mainMenuButton;
    
    // References
    private List<Button> ghostButtons = new List<Button>();
    private List<GameObject> mortalStatusElements = new List<GameObject>();
    private float gameStartTime;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        gameStartTime = Time.time;
        
        // Subscribe to game manager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlasmChanged += UpdatePlasmDisplay;
            GameManager.Instance.OnGhostSelected += UpdateGhostSelection;
            GameManager.Instance.OnLevelComplete += ShowLevelComplete;
            GameManager.Instance.OnMissionCompleted += ShowMissionComplete;
            GameManager.Instance.OnMissionFailed += ShowMissionFailed;
        }
        
        // Set up UI elements
        SetupGhostButtons();
        SetupMortalStatus();
        SetupControls();
        
        // Hide panels initially
        if (ghostInfoPanel != null)
            ghostInfoPanel.SetActive(false);
        if (mortalInfoPanel != null)
            mortalInfoPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (missionCompletePanel != null)
            missionCompletePanel.SetActive(false);
        if (missionFailedPanel != null)
            missionFailedPanel.SetActive(false);
        if (messagePanel != null)
            messagePanel.SetActive(false);
        
        // Set initial objective
        if (objectiveText != null)
        {
            objectiveText.text = "Scare away all mortals!";
        }
        
        Debug.Log("UI Manager initialized");
    }
    
    void SetupGhostButtons()
    {
        if (GameManager.Instance == null || ghostButtonContainer == null || ghostButtonPrefab == null)
            return;
        
        var ghosts = GameManager.Instance.GetAvailableGhosts();
        
        // Clear existing buttons
        foreach (Transform child in ghostButtonContainer)
        {
            Destroy(child.gameObject);
        }
        ghostButtons.Clear();
        
        foreach (var ghost in ghosts)
        {
            GameObject buttonObj = Instantiate(ghostButtonPrefab, ghostButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Set up button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{ghost.GetGhostName()}\n({ghost.GetGhostType()})";
            }
            
            // Set up button click
            button.onClick.AddListener(() => {
                GameManager.Instance.SelectGhost(ghost);
            });
            
            ghostButtons.Add(button);
        }
    }
    
    void SetupMortalStatus()
    {
        if (GameManager.Instance == null || mortalStatusContainer == null || mortalStatusPrefab == null)
            return;
        
        var mortals = GameManager.Instance.GetMortals();
        
        // Clear existing status elements
        foreach (var element in mortalStatusElements)
        {
            if (element != null)
                Destroy(element);
        }
        mortalStatusElements.Clear();
        
        foreach (var mortal in mortals)
        {
            GameObject statusObj = Instantiate(mortalStatusPrefab, mortalStatusContainer);
            
            // Set up mortal name
            TextMeshProUGUI nameText = statusObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{mortal.GetMortalName()} ({mortal.GetMortalType()})";
            }
            
            mortalStatusElements.Add(statusObj);
        }
    }
    
    void SetupControls()
    {
        // Set up power buttons
        if (primaryPowerButton != null)
        {
            primaryPowerButton.onClick.AddListener(() => {
                var selectedGhost = GameManager.Instance?.GetSelectedGhost();
                selectedGhost?.ActivatePrimaryPower();
            });
        }
        
        if (secondaryPowerButton != null)
        {
            secondaryPowerButton.onClick.AddListener(() => {
                var selectedGhost = GameManager.Instance?.GetSelectedGhost();
                selectedGhost?.ActivateSecondaryPower();
            });
        }
        
        // Set up pause button
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        // Set up time scale slider
        if (timeScaleSlider != null)
        {
            timeScaleSlider.value = 1f;
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
        }
        
        // Set up result panel buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }
        
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }
    
    void Update()
    {
        UpdateTimeDisplay();
        UpdateMortalStatus();
        UpdateGhostButtonStates();
    }
    
    void UpdatePlasmDisplay(int currentPlasm)
    {
        if (GameManager.Instance == null) return;
        
        int maxPlasm = GameManager.Instance.maxPlasm;
        
        if (plasmSlider != null)
        {
            plasmSlider.value = (float)currentPlasm / maxPlasm;
        }
        
        if (plasmText != null)
        {
            plasmText.text = $"Plasm: {currentPlasm}/{maxPlasm}";
        }
        
        if (plasmFillImage != null)
        {
            // Change color based on plasm level
            float ratio = (float)currentPlasm / maxPlasm;
            if (ratio > 0.6f)
                plasmFillImage.color = Color.blue;
            else if (ratio > 0.3f)
                plasmFillImage.color = Color.yellow;
            else
                plasmFillImage.color = Color.red;
        }
    }
    
    void UpdateGhostSelection(Ghost selectedGhost)
    {
        if (ghostInfoPanel != null)
        {
            ghostInfoPanel.SetActive(selectedGhost != null);
        }
        
        if (selectedGhost != null)
        {
            if (selectedGhostNameText != null)
            {
                selectedGhostNameText.text = selectedGhost.GetGhostName();
            }
            
            if (selectedGhostTypeText != null)
            {
                selectedGhostTypeText.text = selectedGhost.GetGhostType().ToString();
            }
            
            if (primaryPowerCostText != null)
            {
                primaryPowerCostText.text = $"Cost: {selectedGhost.primaryPowerCost}";
            }
            
            if (secondaryPowerCostText != null)
            {
                secondaryPowerCostText.text = $"Cost: {selectedGhost.secondaryPowerCost}";
            }
        }
        
        // Update ghost button highlights
        UpdateGhostButtonHighlights(selectedGhost);
    }
    
    void UpdateGhostButtonHighlights(Ghost selectedGhost)
    {
        var ghosts = GameManager.Instance?.GetAvailableGhosts();
        if (ghosts == null) return;
        
        for (int i = 0; i < ghostButtons.Count && i < ghosts.Count; i++)
        {
            var button = ghostButtons[i];
            var ghost = ghosts[i];
            
            // Highlight selected ghost button
            var colors = button.colors;
            if (ghost == selectedGhost)
            {
                colors.normalColor = Color.yellow;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            button.colors = colors;
        }
    }
    
    void UpdateMortalStatus()
    {
        if (GameManager.Instance == null) return;
        
        var mortals = GameManager.Instance.GetMortals();
        
        for (int i = 0; i < mortalStatusElements.Count && i < mortals.Count; i++)
        {
            var statusObj = mortalStatusElements[i];
            var mortal = mortals[i];
            
            if (statusObj == null || mortal == null) continue;
            
            // Update fear bar
            Slider fearSlider = statusObj.transform.Find("FearSlider")?.GetComponent<Slider>();
            if (fearSlider != null)
            {
                fearSlider.value = mortal.GetFearRatio();
            }
            
            // Update status text
            TextMeshProUGUI statusText = statusObj.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                string status = "Normal";
                if (mortal.IsScaredAway())
                    status = "Fled";
                else if (mortal.IsPossessed())
                    status = "Possessed";
                else if (mortal.IsStunned())
                    status = "Stunned";
                else if (mortal.IsFrozen())
                    status = "Frozen";
                else if (mortal.GetFearRatio() > 0.8f)
                    status = "Terrified";
                else if (mortal.GetFearRatio() > 0.5f)
                    status = "Scared";
                
                statusText.text = status;
            }
            
            // Gray out if mortal is no longer active
            var canvasGroup = statusObj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = mortal.IsActive() ? 1f : 0.5f;
            }
        }
    }
    
    void UpdateGhostButtonStates()
    {
        if (GameManager.Instance == null) return;
        
        var selectedGhost = GameManager.Instance.GetSelectedGhost();
        int currentPlasm = GameManager.Instance.GetCurrentPlasm();
        
        // Update power button states
        if (primaryPowerButton != null)
        {
            bool canUsePrimary = selectedGhost != null && 
                                selectedGhost.IsBound() && 
                                currentPlasm >= selectedGhost.primaryPowerCost;
            primaryPowerButton.interactable = canUsePrimary;
        }
        
        if (secondaryPowerButton != null)
        {
            bool canUseSecondary = selectedGhost != null && 
                                  selectedGhost.IsBound() && 
                                  currentPlasm >= selectedGhost.secondaryPowerCost;
            secondaryPowerButton.interactable = canUseSecondary;
        }
    }
    
    void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            float elapsedTime = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }
    
    void TogglePause()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsGamePaused())
            {
                GameManager.Instance.ResumeGame();
            }
            else
            {
                GameManager.Instance.PauseGame();
            }
        }
    }
    
    void OnTimeScaleChanged(float value)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.timeScale = value;
        }
    }
    
    void ShowLevelComplete()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            TextMeshProUGUI resultText = gameOverPanel.transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
            if (resultText != null)
            {
                resultText.text = "Level Complete!\nAll mortals have been scared away!";
            }
        }
        
        ShowMessage("Level Complete!");
    }
    
    // Missing method implementations called from MissionManager
    public void ShowMissionComplete(int score)
    {
        if (missionCompletePanel != null)
        {
            missionCompletePanel.SetActive(true);
            
            if (missionCompleteScoreText != null)
            {
                missionCompleteScoreText.text = $"Mission Complete!\nScore: {score}";
            }
        }
        
        ShowMessage($"Mission Complete! Score: {score}");
    }
    
    public void ShowMissionFailed(string reason)
    {
        if (missionFailedPanel != null)
        {
            missionFailedPanel.SetActive(true);
            
            if (missionFailedReasonText != null)
            {
                missionFailedReasonText.text = $"Mission Failed!\n{reason}";
            }
        }
        
        ShowMessage($"Mission Failed: {reason}");
    }
    
    public void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);
            
            // Cancel any existing hide message invoke
            CancelInvoke(nameof(HideMessage));
            
            // Hide message after delay
            Invoke(nameof(HideMessage), messageDisplayTime);
        }
    }
    
    void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    
    // Public methods for UI buttons
    public void RestartLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    public void LoadNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
    }
    
    public void LoadMainMenu()
    {
        // Load the main menu scene (assuming it's at build index 0)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // Utility methods for updating UI
    public void UpdateObjectiveText(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = objective;
        }
    }
    
    public void SetPauseButtonText(string text)
    {
        if (pauseButton != null)
        {
            TextMeshProUGUI buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = text;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlasmChanged -= UpdatePlasmDisplay;
            GameManager.Instance.OnGhostSelected -= UpdateGhostSelection;
            GameManager.Instance.OnLevelComplete -= ShowLevelComplete;
            GameManager.Instance.OnMissionCompleted -= ShowMissionComplete;
            GameManager.Instance.OnMissionFailed -= ShowMissionFailed;
        }
    }
}
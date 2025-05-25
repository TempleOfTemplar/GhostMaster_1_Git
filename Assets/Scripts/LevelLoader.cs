// LevelLoader.cs - Handles level loading and transitions
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Loading Settings")]
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "Gameplay";
    public float loadDelay = 2f;
    
    private static LevelLoader instance;
    public static LevelLoader Instance => instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelCoroutine(sceneName));
    }
    
    public void LoadMainMenu()
    {
        LoadLevel(mainMenuScene);
    }
    
    public void LoadGameplay()
    {
        LoadLevel(gameplayScene);
    }
    
    public void ReloadCurrentLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    
    private IEnumerator LoadLevelCoroutine(string sceneName)
    {
        // Show loading screen if available
        EventSystem.Trigger("ShowLoadingScreen");
        
        yield return new WaitForSeconds(loadDelay);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            EventSystem.Trigger("LoadingProgress", progress);
            yield return null;
        }
        
        EventSystem.Trigger("HideLoadingScreen");
    }
}

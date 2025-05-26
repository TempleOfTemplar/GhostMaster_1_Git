using UnityEngine;

[System.Serializable]
public class MissionObjective
{
    [Header("Objective Settings")]
    public string objectiveName = "Unnamed Objective";
    public ObjectiveType type = ObjectiveType.ScareAllMortals;
    public int targetValue = 1;
    public bool isOptional = false;
    
    [Header("Current Status")]
    public int currentProgress = 0;
    public bool isCompleted = false;
    
    // Events
    public System.Action<MissionObjective> OnObjectiveCompleted;
    public System.Action<MissionObjective> OnProgressChanged;
    
    public void SetProgress(int progress)
    {
        int oldProgress = currentProgress;
        currentProgress = progress;
        
        // Notify progress change
        if (oldProgress != currentProgress)
        {
            OnProgressChanged?.Invoke(this);
        }
        
        // Check if objective is now completed
        if (!isCompleted && currentProgress >= targetValue)
        {
            CompleteObjective();
        }
    }
    
    public void CompleteObjective()
    {
        if (isCompleted) return;
        
        isCompleted = true;
        OnObjectiveCompleted?.Invoke(this);
    }
    
    public float GetProgressRatio()
    {
        if (targetValue <= 0) return 1f;
        return Mathf.Clamp01((float)currentProgress / targetValue);
    }
    
    public string GetProgressText()
    {
        switch (type)
        {
            case ObjectiveType.ScareAllMortals:
                return $"Scare Mortals: {currentProgress}/{targetValue}";
            case ObjectiveType.CollectPlasm:
                return $"Collect Plasm: {currentProgress}/{targetValue}";
            case ObjectiveType.SurviveTime:
                return $"Survive Time: {currentProgress}s/{targetValue}s";
            default:
                return $"{objectiveName}: {currentProgress}/{targetValue}";
        }
    }
}

public enum ObjectiveType
{
    ScareAllMortals,
    CollectPlasm,
    SurviveTime,
    PossessMortals,
    UseSpecificPower,
    DefeatBoss,
    FindSecrets,
    NoMortalsEscape
}
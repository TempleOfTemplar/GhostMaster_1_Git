// MissionObjective.cs - Defines mission goals
using UnityEngine;
using System;

[System.Serializable]
public enum ObjectiveType
{
    ScareAllMortals,
    ScareSpecificMortal,
    SurviveTime,
    CollectPlasm,
    DestroyObjects,
    PreventMortalEscape
}

[System.Serializable]
public class MissionObjective
{
    public string objectiveName;
    public string description;
    public ObjectiveType type;
    public int targetValue;
    public int currentValue;
    public bool isCompleted;
    public bool isOptional;
    
    public event Action<MissionObjective> OnObjectiveCompleted;
    
    public void UpdateProgress(int value)
    {
        currentValue = Mathf.Min(currentValue + value, targetValue);
        CheckCompletion();
    }
    
    public void SetProgress(int value)
    {
        currentValue = Mathf.Min(value, targetValue);
        CheckCompletion();
    }
    
    private void CheckCompletion()
    {
        if (!isCompleted && currentValue >= targetValue)
        {
            isCompleted = true;
            OnObjectiveCompleted?.Invoke(this);
        }
    }
    
    public float GetProgressPercentage()
    {
        return targetValue > 0 ? (float)currentValue / targetValue : 0f;
    }
}

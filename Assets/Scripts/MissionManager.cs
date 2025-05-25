// MissionManager.cs - Manages mission objectives and completion
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MissionManager : MonoBehaviour
{
    [Header("Mission Settings")]
    public string missionName = "Haunt the House";
    public string missionDescription = "Scare all the mortals out of the house";
    public List<MissionObjective> objectives = new List<MissionObjective>();
    
    [Header("Mission Results")]
    public bool missionCompleted = false;
    public bool missionFailed = false;
    
    private GameManager gameManager;
    private UIManager uiManager;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        // Subscribe to objective completion events
        foreach (var objective in objectives)
        {
            objective.OnObjectiveCompleted += OnObjectiveCompleted;
        }
        
        // Initialize objective tracking
        StartObjectiveTracking();
    }
    
    private void StartObjectiveTracking()
    {
        InvokeRepeating(nameof(UpdateObjectiveProgress), 1f, 1f);
    }
    
    private void UpdateObjectiveProgress()
    {
        if (missionCompleted || missionFailed) return;
        
        foreach (var objective in objectives)
        {
            if (objective.isCompleted) continue;
            
            switch (objective.type)
            {
                case ObjectiveType.ScareAllMortals:
                    UpdateScareAllMortalsObjective(objective);
                    break;
                case ObjectiveType.CollectPlasm:
                    UpdateCollectPlasmObjective(objective);
                    break;
                case ObjectiveType.SurviveTime:
                    UpdateSurviveTimeObjective(objective);
                    break;
            }
        }
    }
    
    private void UpdateScareAllMortalsObjective(MissionObjective objective)
    {
        Mortal[] mortals = FindObjectsOfType<Mortal>();
        int scaredMortals = mortals.Count(m => m.GetFearLevel() > 80f || m.HasFled());
        objective.SetProgress(scaredMortals);
    }
    
    private void UpdateCollectPlasmObjective(MissionObjective objective)
    {
        Ghost[] ghosts = FindObjectsOfType<Ghost>();
        int totalPlasm = ghosts.Sum(g => Mathf.RoundToInt(g.GetPlasm()));
        objective.SetProgress(totalPlasm);
    }
    
    private void UpdateSurviveTimeObjective(MissionObjective objective)
    {
        int survivedTime = Mathf.RoundToInt(Time.time);
        objective.SetProgress(survivedTime);
    }
    
    private void OnObjectiveCompleted(MissionObjective objective)
    {
        Debug.Log($"Objective completed: {objective.objectiveName}");
        
        // Check if all mandatory objectives are completed
        var mandatoryObjectives = objectives.Where(o => !o.isOptional);
        if (mandatoryObjectives.All(o => o.isCompleted))
        {
            CompleteMission();
        }
    }
    
    public void CompleteMission()
    {
        if (missionCompleted) return;
        
        missionCompleted = true;
        CancelInvoke(nameof(UpdateObjectiveProgress));
        
        Debug.Log("Mission Completed!");
        
        // Calculate score based on objectives
        int score = CalculateMissionScore();
        
        if (gameManager != null)
            gameManager.OnMissionCompleted(score);
            
        if (uiManager != null)
            uiManager.ShowMissionComplete(score);
    }
    
    public void FailMission(string reason = "")
    {
        if (missionFailed || missionCompleted) return;
        
        missionFailed = true;
        CancelInvoke(nameof(UpdateObjectiveProgress));
        
        Debug.Log($"Mission Failed! {reason}");
        
        if (gameManager != null)
            gameManager.OnMissionFailed(reason);
            
        if (uiManager != null)
            uiManager.ShowMissionFailed(reason);
    }
    
    private int CalculateMissionScore()
    {
        int baseScore = 1000;
        int objectiveBonus = objectives.Count(o => o.isCompleted) * 500;
        int optionalBonus = objectives.Count(o => o.isOptional && o.isCompleted) * 250;
        
        return baseScore + objectiveBonus + optionalBonus;
    }
    
    public List<MissionObjective> GetObjectives()
    {
        return objectives;
    }
    
    public bool IsMissionActive()
    {
        return !missionCompleted && !missionFailed;
    }
}
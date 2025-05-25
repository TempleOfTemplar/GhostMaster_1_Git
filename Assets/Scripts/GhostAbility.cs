// GhostAbility.cs - Defines special abilities for ghosts
using UnityEngine;
using System.Collections;

[System.Serializable]
public class GhostAbility
{
    public string abilityName;
    public string description;
    public float plasmCost;
    public float cooldownTime;
    public Sprite abilityIcon;
    
    [HideInInspector]
    public float lastUsedTime;
    
    public bool CanUse()
    {
        return Time.time >= lastUsedTime + cooldownTime;
    }
    
    public void Use()
    {
        lastUsedTime = Time.time;
    }
    
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, (lastUsedTime + cooldownTime) - Time.time);
    }
}

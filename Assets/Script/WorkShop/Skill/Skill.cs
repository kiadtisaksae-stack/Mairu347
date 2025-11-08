using System;
using UnityEngine;

public abstract class Skill
{
    public string skillName;
    public float cooldownTime;
    public float lastUsedTime = float.MinValue; 
    public float timer; 


    public abstract void Activate(Character character);
    public abstract void Deactivate(Character character);
    public abstract void UpdateSkill(Character character);
    public void ResetCooldown()
    {
        lastUsedTime = float.MinValue; 
    }
    public bool IsReady(float GameTime)
    {
        return GameTime >= lastUsedTime + cooldownTime;
    }

    public void TimeStampSkill(float GameTime)
    {
        lastUsedTime = GameTime;
    }

    public void DisplayInfo()
    {
        Debug.Log($"Skill: {skillName}");
        Debug.Log($"Cooldown: {cooldownTime}s");
    }
}

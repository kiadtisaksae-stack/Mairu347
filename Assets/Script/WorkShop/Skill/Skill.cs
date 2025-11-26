using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Skill : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Vector3 casterPosition;
    [Header("Skill Timing")]
    public float cooldownTime;
    public float lastUsedTime = float.MinValue; 
    public float timer;
    public float lifeTime = 2;
    [Header("Skill Type")]
    public bool isPassive;
    public bool isRange;
    [Header("Skill Tree Cost")]
    public int skillPointCost;
    public List<Skill> skillrequire;
    [Header("Skill Visual")]
    public Sprite skillIcon;
    public GameObject skillPrefab;

    
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

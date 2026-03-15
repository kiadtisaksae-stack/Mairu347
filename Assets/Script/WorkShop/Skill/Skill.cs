using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public string skillName;
    public string skillDescription;
    public Vector3 casterPosition;

    [Header("Skill Timing")]
    public float cooldownTime;
    // ลบ lastUsedTime ออกจาก ScriptableObject
    // เพราะ ScriptableObject เป็น Shared Asset — ทุก Player ใช้ชิ้นเดียวกัน
    // cooldown tracking ย้ายไปเก็บใน SkillBook แทน
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

    // Activate รับ instance ของ GameObject ที่ Instantiate แล้ว
    // เพื่อให้ set component บน instance จริง ไม่ใช่บน Prefab Asset
    public abstract void Activate(Character character, GameObject spawnedInstance);
    public abstract void Deactivate(Character character);
    public abstract void UpdateSkill(Character character);

    public void DisplayInfo()
    {
        Debug.Log($"Skill: {skillName}");
        Debug.Log($"Cooldown: {cooldownTime}s");
    }
}
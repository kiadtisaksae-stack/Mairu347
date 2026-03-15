using UnityEngine;

[CreateAssetMenu(fileName = "MeteorRainSkill", menuName = "Skills/MeteorRain")]
public class MeteorRain : Skill
{
    [Header("Skill Ability")]
    public float skillRadius;
    public int damageToHit;
    public float duration;

    public MeteorRain()
    {
        this.skillName = "Meteor Rain";
        // แก้ปัญหา #25 — เดิม lifeTime = duration แต่ duration ยังเป็น 0 ตอน Constructor
        // ย้ายไปตั้งใน Activate แทน
        skillRadius = 10;
        this.cooldownTime = 30f;
    }

    // รับ spawnedInstance ที่ Instantiate แล้วจาก SkillBook
    // แก้ปัญหา #2 — เดิม GetComponent จาก skillPrefab (Prefab Asset)
    public override void Activate(Character character, GameObject spawnedInstance)
    {
        if (spawnedInstance == null)
        {
            Debug.LogWarning($"[{skillName}] spawnedInstance is null!");
            return;
        }

        Hit hit = spawnedInstance.GetComponent<Hit>();
        if (hit == null)
        {
            Debug.LogWarning($"[{skillName}] No Hit component on spawned instance!");
            return;
        }

        hit.damagePerTick = this.damageToHit;
        hit.radius = this.skillRadius;

        // ตั้ง lifeTime ตอน Activate เพราะ duration ถูก set ใน Inspector แล้ว
        this.lifeTime = duration;
        timer = duration;
    }

    public override void Deactivate(Character character)
    {
        Debug.Log("Meteor skill duration ended.");
    }

    public override void UpdateSkill(Character character) { }
}
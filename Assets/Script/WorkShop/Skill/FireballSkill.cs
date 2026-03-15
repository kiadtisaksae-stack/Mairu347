using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireballSkill", menuName = "Skills/FireballSkill")]
public class FireballSkill : Skill
{
    [Header("Skill Ability")]
    public int damage = 50;
    public float searchRadius = 5;

    public FireballSkill()
    {
        this.skillName = "FireballSkill";
        this.cooldownTime = 5;
    }

    // รับ spawnedInstance จาก SkillBook (ไม่ใช้ในสกิลนี้ แต่ต้อง implement ตาม abstract)
    public override void Activate(Character character, GameObject spawnedInstance)
    {
        Debug.Log(character.Name + " Casting Fireball! Deals " + damage + " damage.");

        // แก้ปัญหา #7 — เดิมเรียก enemy.TakeDamage() จาก Client โดยตรง
        // TakeDamage เช็ค if (!IsServer) return → damage ไม่เกิดขึ้นเลย
        // แก้โดยส่งผ่าน DealDamageServerRpc ของ Player แทน

        Player player = character as Player;
        if (player == null)
        {
            Debug.LogWarning("[FireballSkill] Caster is not a Player, cannot send ServerRpc");
            return;
        }

        Enemy[] targets = GetEnemiesInRange(character);
        if (targets.Length > 0)
        {
            foreach (var enemy in targets)
            {
                // ส่ง damage ผ่าน ServerRpc ของ Player
                // DealDamageServerRpc จะทำงานบน Server → เรียก TakeDamage บน Server ถูกต้อง
                player.DealDamageServerRpc(enemy.NetworkObjectId, damage);
                Debug.Log($"{character.Name} casts {skillName} on {enemy.Name}, dealing {damage} damage!");
            }
        }
        else
        {
            Debug.Log("No enemies in range to target with Fireball.");
        }
    }

    public override void Deactivate(Character character) { }

    public override void UpdateSkill(Character character) { }

    private Enemy[] GetEnemiesInRange(Character caster)
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, searchRadius);
        List<Enemy> enemies = new List<Enemy>();

        foreach (var hitCollider in hitColliders)
        {
            Enemy targetCharacter = hitCollider.GetComponent<Enemy>();
            if (targetCharacter != null && targetCharacter != caster)
            {
                enemies.Add(targetCharacter);
            }
        }
        return enemies.ToArray();
    }
}
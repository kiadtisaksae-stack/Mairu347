using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightingAuraSkill", menuName = "Skills/LightingAura")]
public class LightingAuraSkill : Skill
{
    [Header("Skill Ability")]
    public int lightingDamage = 5;
    private float damageAccumulator = 0f;
    public float duration;
    public float lightingRadius;

    [Header("Skill Buff")]
    public float speedIncreaseAmount = 5f;
    float originalSpeed;
    float targetSpeed;
    public float addCasterDamage = 2f;
    int originalDamage;
    int targetDamage;

    public LightingAuraSkill()
    {
        this.lifeTime = duration;
    }

    // แก้ signature เพิ่ม GameObject spawnedInstance — สกิลนี้ไม่ใช้ instance
    public override void Activate(Character character, GameObject spawnedInstance)
    {
        Debug.Log("Casting Lighting Aura!");
        timer = duration;

        originalSpeed = character.movementSpeed;
        originalDamage = character.Damage;
        targetSpeed = originalSpeed + speedIncreaseAmount;
        targetDamage = (int)(originalDamage * addCasterDamage);
    }

    public override void Deactivate(Character character)
    {
        character.movementSpeed = originalSpeed;
        character.Damage = originalDamage;
        Debug.Log("Lighting Aura! skill duration ended.");
    }

    public override void UpdateSkill(Character character)
    {
        timer -= Time.deltaTime;
        character.movementSpeed = targetSpeed;
        character.Damage = targetDamage;

        if (timer >= 0)
        {
            damageAccumulator += Time.deltaTime;
            if (damageAccumulator >= 1)
            {
                Enemy[] enemies = GetEnemysInRange(character);
                foreach (var enemy in enemies)
                {
                    // ส่ง damage ผ่าน ServerRpc ถ้าเป็น Player
                    Player player = character as Player;
                    if (player != null)
                        player.DealDamageServerRpc(enemy.NetworkObjectId, lightingDamage);
                    else
                        enemy.TakeDamage(lightingDamage);

                    Debug.Log($"{character.Name} lighting strike to {enemy.name} {lightingDamage} damage. Remaining: {timer:F2}s");
                }
                damageAccumulator = 0;
            }
        }
        else
        {
            Deactivate(character);
        }
    }

    private Enemy[] GetEnemysInRange(Character caster)
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, lightingRadius);
        List<Enemy> enemies = new List<Enemy>();
        foreach (var hitCollider in hitColliders)
        {
            Enemy target = hitCollider.GetComponent<Enemy>();
            if (target != null && target != caster)
                enemies.Add(target);
        }
        return enemies.ToArray();
    }
}
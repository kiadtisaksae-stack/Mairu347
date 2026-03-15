using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HolyDomainSkill", menuName = "Skills/HolyDomainSkill")]
public class HolyDomainSkill : Skill
{
    [Header("Skill Ability")]
    public int healingAmountPerSecond = 10;
    public float skillRadius;
    public float duration;
    private float healAccumulator = 0f;

    [Header("Skill Debuff")]
    public float reduceDamage = 0.4f;
    public float reduceSpeed = 0.5f;

    public HolyDomainSkill()
    {
        this.skillName = "Holy Domain";
        this.cooldownTime = 30;
        this.lifeTime = duration;
    }

    // แก้ signature เพิ่ม GameObject spawnedInstance — สกิลนี้ไม่ใช้ instance
    public override void Activate(Character character, GameObject spawnedInstance)
    {
        timer = duration;

        Enemy[] enemies = GetEnemysInRange(character);
        if (enemies.Length > 0)
        {
            foreach (var enemy in enemies)
            {
                AddDebuff(enemy);
                Debug.Log($"{character.Name} casts {skillName} on {enemy.Name}, Debuffing! now {enemy.Damage} damage and speed {enemy.movementSpeed}");
            }
        }
    }

    public override void Deactivate(Character character) { }

    public override void UpdateSkill(Character character)
    {
        timer -= Time.deltaTime;
        if (timer >= 0)
        {
            healAccumulator += Time.deltaTime;
            if (healAccumulator >= 1)
            {
                Player[] players = GetPlayerInRange(character);
                foreach (var targetplayer in players)
                {
                    targetplayer.Heal(healingAmountPerSecond);
                    Debug.Log($"{targetplayer.Name} heals for {healingAmountPerSecond} HP. Remaining: {timer:F2}s");
                }
                healAccumulator = 0;
            }
        }
        else
        {
            Deactivate(character);
        }
    }

    private void AddDebuff(Enemy enemy)
    {
        int originalDamage = enemy.Damage;
        float originalSpeed = enemy.movementSpeed;
        if (timer >= 0)
        {
            enemy.Damage = (int)(originalDamage * 0.4f);
            enemy.movementSpeed = originalSpeed * 0.5f;
        }
        else
        {
            enemy.Damage = originalDamage;
            enemy.movementSpeed = originalSpeed;
            Debug.Log($"{skillName} on {enemy.Name}, Stop Debuffing");
        }
    }

    private Enemy[] GetEnemysInRange(Character caster)
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, skillRadius);
        List<Enemy> enemies = new List<Enemy>();
        foreach (var hitCollider in hitColliders)
        {
            Enemy target = hitCollider.GetComponent<Enemy>();
            if (target != null && target != caster)
                enemies.Add(target);
        }
        return enemies.ToArray();
    }

    private Player[] GetPlayerInRange(Character caster)
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, skillRadius);
        List<Player> players = new List<Player>();
        foreach (var hitCollider in hitColliders)
        {
            Player player = hitCollider.GetComponent<Player>();
            if (player != null)
                players.Add(player);
        }
        return players.ToArray();
    }
}
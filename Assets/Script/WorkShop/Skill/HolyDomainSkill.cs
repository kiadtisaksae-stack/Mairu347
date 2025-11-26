using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
    //int originalDamage;
    //int TargetDamage;
    public float reduceSpeed = 0.5f;
    //float OriginalSpeed;
    //float TargetSpeed;

    public HolyDomainSkill()
    {
        this.skillName = "Holy Domain";
        this.cooldownTime = 30;
        this.lifeTime = duration;
    }


    public override void Activate(Character character)
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

    public override void Deactivate(Character character)
    {
       
    }

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
                    Debug.Log($"{targetplayer.Name} heals for {healingAmountPerSecond} HP. Remaining Duration: {timer:F2} seconds.");
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
        // Find all colliders within the search radius
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, skillRadius);
        List<Enemy> Enemys = new List<Enemy>();

        foreach (var hitCollider in hitColliders)
        {
            // Check if the collider belongs to a character that isn't the caster
            Enemy targetCharacter = hitCollider.GetComponent<Enemy>();
            if (targetCharacter != null && targetCharacter != caster)
            {
                Enemys.Add(targetCharacter);
            }
        }
        return Enemys.ToArray();
    }

    private Player[] GetPlayerInRange(Character caster)
    {
        Collider[] hitcolliders = Physics.OverlapSphere(caster.transform.position, skillRadius);
        List<Player> players = new List<Player>();
        foreach (var hitcollider in hitcolliders)
        {
            Player inRangeplayer = hitcollider.GetComponent<Player>();
            if (inRangeplayer != null)
            {
                players.Add(inRangeplayer);
            }
        }
        return players.ToArray();
    }
}

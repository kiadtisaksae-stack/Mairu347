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


    public override void Activate(Character character)
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
                    enemy.TakeDamage(lightingDamage);
                    Debug.Log($"{character.Name} lighting strike to {enemy.name} {lightingDamage} damage. Remaining Duration: {timer:F2} seconds.");
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
        // Find all colliders within the search radius
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, lightingRadius);
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
}

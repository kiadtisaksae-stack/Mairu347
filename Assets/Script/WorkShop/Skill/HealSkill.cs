using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "Skills/HealSkill")]
public class HealSkill : Skill
{
    public int healingAmountPerSecond = 5;

    private float healAccumulator = 0f;
    public HealSkill()
    {
        this.skillName = "Heal";
        this.cooldownTime = 8;
        this.Duration = 5f; 
    }

    public float Duration { get; set; }

    public override void Activate(Character character)
    {
        Debug.Log("Casting Heal Over Time!");
        timer = Duration;
    }

    public override void Deactivate(Character character)
    {
        Debug.Log("Heal skill duration ended.");
    }

    public override void UpdateSkill(Character character)
    {
        timer -= Time.deltaTime;
        if (timer > 0)
        {
            healAccumulator += Time.deltaTime;

            if (healAccumulator >= 1)
            {
                character.Heal(healingAmountPerSecond);
                healAccumulator = 0;
                Debug.Log($"{character.Name} heals for {healingAmountPerSecond} HP. Remaining Duration: {timer:F2} seconds.");
            }
        }
        else
        {
            Deactivate(character);
        }
    }
}

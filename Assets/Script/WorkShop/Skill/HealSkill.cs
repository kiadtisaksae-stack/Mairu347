using UnityEngine;

[CreateAssetMenu(fileName = "HealSkill", menuName = "Skills/HealSkill")]
public class HealSkill : Skill
{
    [Header("Skill Ability")]
    public int healingAmountPerSecond = 5;
    private float healAccumulator = 0f;
    public float Duration;

    public HealSkill()
    {
        this.skillName = "Heal";
        this.cooldownTime = 8;
        this.Duration = 5f;
    }

    // แก้ signature เพิ่ม GameObject spawnedInstance — สกิลนี้ไม่ใช้ instance
    public override void Activate(Character character, GameObject spawnedInstance)
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
                Debug.Log($"{character.Name} heals for {healingAmountPerSecond} HP. Remaining: {timer:F2}s");
            }
        }
        else
        {
            Deactivate(character);
        }
    }
}
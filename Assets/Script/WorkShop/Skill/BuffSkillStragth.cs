using UnityEngine;


[CreateAssetMenu(fileName = "BuffSkillStrength", menuName = "Skills/BuffSkillStrength")]
public class BuffSkillStragth : Skill
{

    [Header("Skill Ability")]
    public float addDamage = 1.2f;
    int originalDamage;
    int targetDamage;

    public int addDefence;
    int orignalDefence;
    int targetDefence;

    public float duration;

    public BuffSkillStragth()
    {
        this.skillName = "Strength Buff";
        this.cooldownTime = 15;
        this.duration = 10f; 
    }


    public override void Activate(Character character)
    {
        timer = duration;

        originalDamage = character.Damage;
        orignalDefence = character.Defence;
        targetDamage = (int)(originalDamage * addDamage);
        targetDefence = orignalDefence + addDefence;
        Debug.Log($"{character.Name} Atk increased by {targetDamage} and Def {targetDefence} for {duration} seconds.");

    }

    public override void Deactivate(Character character)
    {
        character.Damage = originalDamage;
        character.Defence = orignalDefence;
        Debug.Log($"{character.Name}'s Strength boost has ended.");
    }

    public override void UpdateSkill(Character character)
    {
        timer -= Time.deltaTime;
        character.Damage = targetDamage;
        character.Defence = targetDefence;
        if (timer <= 0)
        {
            Deactivate(character);
        }
    }

}

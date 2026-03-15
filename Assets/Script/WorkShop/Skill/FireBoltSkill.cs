using UnityEngine;

[CreateAssetMenu(fileName = "FireBoltSkill", menuName = "Skills/SlashSkill")]
public class FireBoltSkill : Skill
{
    [Header("Skill Ability")]
    public int damage;
    public float firespeed = 20f;

    public FireBoltSkill()
    {
        this.skillName = "Fire Bolt Shot";
        this.cooldownTime = 5f;
    }

    public override void Activate(Character character, GameObject spawnedInstance)
    {
        if (spawnedInstance == null)
        {
            Debug.LogWarning($"[{skillName}] spawnedInstance is null!");
            return;
        }

        Projectle projectle = spawnedInstance.GetComponent<Projectle>();
        if (projectle == null)
        {
            Debug.LogWarning($"[{skillName}] No Projectle component on spawned instance!");
            return;
        }

        projectle.character = character;
        projectle.damage = this.damage;
        projectle.speed = this.firespeed;
        projectle.lifetime = this.lifeTime;
    }

    public override void Deactivate(Character character) { }

    public override void UpdateSkill(Character character) { }
}
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "FireSlashSkill", menuName = "Skills/SlashSkill")]

public class FireBoltSkill : Skill
{

    [Header("Skill Ability")]
    public int damage;
    public float firespeed = 20f;

    private Rigidbody rb;

    public FireBoltSkill()
    {
        this.skillName = "Fire Bolt Shot";
        this.cooldownTime = 5f;
    }

    public override void Activate(Character character)
    {
        Projectle projectle = skillPrefab.GetComponent<Projectle>();
        projectle.character = character;
        projectle.damage = this.damage;
        projectle.speed = this.firespeed;
        projectle.lifetime = this.lifeTime;
    }

    public override void Deactivate(Character character)
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateSkill(Character character)
    {

    }

}

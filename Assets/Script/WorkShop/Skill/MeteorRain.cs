using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MeteorRainSkill", menuName = "Skills/MeteorRain")]
public class MeteorRain : Skill
{
    [Header("Skill Ability")]
    public float skillRadius;
    public int damageToHit;
    private float damageAccumulator = 0f;
    public float duration;

    public MeteorRain()
    {
        this.skillName = "Meteor Rain";
        this.lifeTime = duration;
        skillRadius = 10;
        this.cooldownTime = 30f;
    }

    public override void Activate(Character character)
    {
        Hit hit = skillPrefab.GetComponent<Hit>();
        hit.damagePerTick = this.damageToHit;
        hit.radius = this.skillRadius;
        timer = duration;
    }

    public override void Deactivate(Character character)
    {
        Debug.Log("Meteor skill duration ended.");
    }

    public override void UpdateSkill(Character character)
    {
    
    }
}

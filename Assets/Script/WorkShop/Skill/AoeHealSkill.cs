using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AoeHealSkill", menuName = "Skills/AoeHealSkill")]
public class AoeHealSkill : Skill
{
    [Header("Skill Ability")]
    public int healAmount = 50;
    public float healRadius = 10;

    public AoeHealSkill()
    {
        this.skillName = "Heal Area";
        this.cooldownTime = 30;
    }

    // แก้ signature เพิ่ม GameObject spawnedInstance — สกิลนี้ไม่ใช้ instance
    public override void Activate(Character character, GameObject spawnedInstance)
    {
        Debug.Log($"{character.name} casting Aoe Heal skill deal {healAmount}");

        Player[] players = GetPlayerInRange(character);
        foreach (var p in players)
        {
            p.Heal(healAmount);
            Debug.Log($"{p.name} take heal {healAmount}");
        }
    }

    public override void Deactivate(Character character) { }

    public override void UpdateSkill(Character character) { }

    private Player[] GetPlayerInRange(Character caster)
    {
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, healRadius);
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
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillBook : NetworkBehaviour
{
    public List<Skill> skillsSet = new List<Skill>();
    public GameObject[] skillEffects; // Prefabs ของ effect แต่ละสกิล

    private List<Skill> DulationSkills = new List<Skill>();

    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();

        // ✅ เตรียมสกิล
        skillsSet.Add(new FireballSkill());
        skillsSet.Add(new HealSkill());
        skillsSet.Add(new BuffSkillMoveSpeed());
    }


    void Update()
    {
        for (int i = DulationSkills.Count - 1; i >= 0; i--)
        {
            DulationSkills[i].UpdateSkill(player);

            if (DulationSkills[i].timer <= 0)
                DulationSkills.RemoveAt(i);
        }
    }

    // ✅ Owner เรียกใช้ก่อน
    public void UseSkill(int index)
    {
        if (!IsOwner) return;
        UseSkillServerRpc(index);
    }

    [ServerRpc]
    private void UseSkillServerRpc(int index)
    {
        ExecuteSkill(index);

        UseSkillClientRpc(index);
    }
    [ClientRpc]
    private void UseSkillClientRpc(int index)
    {
        if (!IsServer)
            ExecuteSkill(index);
    }
    private void ExecuteSkill(int index)
    {
        if (index < 0 || index >= skillsSet.Count)
            return;

        Skill skill = skillsSet[index];

        if (!skill.IsReady(Time.time))
        {
            Debug.Log($"Skill '{skill.skillName}' cooldown: {skill.lastUsedTime + skill.cooldownTime - Time.time:F2}s");
            return;
        }

        // Spawn Effect
        GameObject g = Instantiate(skillEffects[index], transform.position, Quaternion.identity, transform);
        Destroy(g, 2);

        // Run Skill Action
        skill.Activate(player);
        skill.TimeStampSkill(Time.time);

        if (skill.timer > 0)
            DulationSkills.Add(skill);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}

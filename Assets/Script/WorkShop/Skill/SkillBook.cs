using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SkillBook : NetworkBehaviour
{
    public List<Skill> skillsSet = new List<Skill>();

    private List<Skill> DurationSkills = new List<Skill>();
    private Dictionary<Skill, float> _skillLastUsedTime = new Dictionary<Skill, float>();

    private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        player = GetComponent<Player>();
    }

    void Update()
    {
        for (int i = DurationSkills.Count - 1; i >= 0; i--)
        {
            DurationSkills[i].UpdateSkill(player);
            if (DurationSkills[i].timer <= 0)
                DurationSkills.RemoveAt(i);
        }
    }

    public void UseSkill(int index)
    {
        if (!IsOwner) return;
        if (index < 0 || index >= skillsSet.Count) return;

        Skill skill = skillsSet[index];

        if (!IsSkillReady(skill))
        {
            Debug.Log($"[SkillBook] '{skill.skillName}' cooldown เหลือ {GetCooldownRemaining(skill):F2}s");
            return;
        }

        // Owner รัน visual เอง
        SpawnVisualLocally(skill, player.transform.position, player.transform.rotation);

        // ส่งชื่อสกิลไปแทน index — เพราะ Client อื่นอาจมี skillsSet ต่างกัน
        NotifySkillUsedServerRpc(skill.skillName, player.transform.position, player.transform.rotation);

        _skillLastUsedTime[skill] = Time.time;
    }

    [ServerRpc]
    private void NotifySkillUsedServerRpc(string skillName, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[SERVER] SkillBook received: {skillName} at {position}");
        BroadcastSkillUsedClientRpc(skillName, position, rotation);
    }

    [ClientRpc]
    private void BroadcastSkillUsedClientRpc(string skillName, Vector3 position, Quaternion rotation)
    {
        if (IsOwner) return;

        // ✅ ค้นหา prefab จาก SkillTreeManager โดยตรงด้วยชื่อสกิล
        // ไม่ต้องพึ่ง skillsSet ของ Client อื่น ซึ่งอาจว่างเปล่า
        GameObject prefab = SkillTreeManager.instance?.GetSkillPrefabByName(skillName);

        if (prefab == null)
        {
            Debug.LogWarning($"[SkillBook] Client ไม่พบ prefab สำหรับ '{skillName}'");
            return;
        }

        Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] spawning visual for '{skillName}'");

        GameObject instance = Instantiate(prefab, position, rotation);

        // หา Skill SO จาก registry เพื่อดึงค่า lifeTime
        Skill skill = SkillTreeManager.instance?.GetSkillByName(skillName);
        if (skill != null)
        {
            if (skill.isRange)
                instance.transform.position = position + rotation * new Vector3(0, 0, skill.casterPosition.z);

            skill.Activate(player, instance);

            if (!skill.isPassive)
                instance.transform.SetParent(null);

            Destroy(instance, skill.lifeTime);

            if (skill.timer > 0)
                DurationSkills.Add(skill);
        }
        else
        {
            Destroy(instance, 2f); // fallback
        }
    }

    private void SpawnVisualLocally(Skill skill, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = SkillTreeManager.instance != null
            ? SkillTreeManager.instance.GetSkillPrefab(skill)
            : skill.skillPrefab;

        if (prefab == null)
        {
            Debug.LogWarning($"[SkillBook] ไม่พบ prefab สำหรับ '{skill.skillName}'");
            return;
        }

        GameObject instance = Instantiate(prefab, position, rotation);

        if (skill.isRange)
            instance.transform.position = position + rotation * new Vector3(0, 0, skill.casterPosition.z);

        skill.Activate(player, instance);

        if (!skill.isPassive)
            instance.transform.SetParent(null);

        Destroy(instance, skill.lifeTime);

        if (skill.timer > 0)
            DurationSkills.Add(skill);
    }

    private bool IsSkillReady(Skill skill)
    {
        if (!_skillLastUsedTime.ContainsKey(skill)) return true;
        return Time.time >= _skillLastUsedTime[skill] + skill.cooldownTime;
    }

    private float GetCooldownRemaining(Skill skill)
    {
        if (!_skillLastUsedTime.ContainsKey(skill)) return 0f;
        return Mathf.Max(0f, (_skillLastUsedTime[skill] + skill.cooldownTime) - Time.time);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
}
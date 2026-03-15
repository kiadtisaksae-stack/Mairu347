using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager instance;

    public HashSet<Skill> unlockSkills = new HashSet<Skill>();
    public int currentskillPoint;
    public PlayerLevel playerLevel;
    public System.Action OnSkillTreeChanged;

    public GameObject skillTreeUI;
    private bool isActive;
    public TMP_Text skillPointText;

    [Header("Skill Prefabs Registry")]
    public List<SkillPrefabEntry> skillPrefabRegistry = new List<SkillPrefabEntry>();

    [System.Serializable]
    public class SkillPrefabEntry
    {
        public Skill skill;
        public GameObject prefab;
    }

    // lookup by Skill SO
    private Dictionary<Skill, GameObject> _prefabBySkill;
    // ✅ lookup by name — ให้ Client อื่น query ได้โดยไม่ต้องมี skillsSet
    private Dictionary<string, GameObject> _prefabByName;
    private Dictionary<string, Skill> _skillByName;

    private void Awake()
    {
        instance = this;

        _prefabBySkill = new Dictionary<Skill, GameObject>();
        _prefabByName = new Dictionary<string, GameObject>();
        _skillByName = new Dictionary<string, Skill>();

        foreach (var entry in skillPrefabRegistry)
        {
            if (entry.skill == null || entry.prefab == null) continue;

            _prefabBySkill[entry.skill] = entry.prefab;
            _prefabByName[entry.skill.skillName] = entry.prefab;
            _skillByName[entry.skill.skillName] = entry.skill;
        }
    }

    void Start()
    {
        UpdateSkillPointText();
        skillTreeUI.SetActive(false);
    }

    // ── Query methods ──────────────────────────────

    public GameObject GetSkillPrefab(Skill skill)
    {
        if (_prefabBySkill != null && _prefabBySkill.TryGetValue(skill, out var prefab))
            return prefab;

        Debug.LogWarning($"[SkillTreeManager] ไม่พบ prefab ของ '{skill.skillName}' — fallback ไป skill.skillPrefab");
        return skill.skillPrefab;
    }

    // ✅ ใหม่ — query ด้วยชื่อ ให้ Client อื่นใช้ใน ClientRpc
    public GameObject GetSkillPrefabByName(string skillName)
    {
        if (_prefabByName != null && _prefabByName.TryGetValue(skillName, out var prefab))
            return prefab;

        Debug.LogWarning($"[SkillTreeManager] ไม่พบ prefab ของ '{skillName}' ใน registry");
        return null;
    }

    // ✅ ใหม่ — คืน Skill SO ด้วยชื่อ
    public Skill GetSkillByName(string skillName)
    {
        if (_skillByName != null && _skillByName.TryGetValue(skillName, out var skill))
            return skill;
        return null;
    }

    // ── Skill Tree logic ──────────────────────────

    public void SetupForLocalPlayer(PlayerLevel localLevel)
    {
        if (playerLevel != null)
            playerLevel.OnLevelUp.RemoveListener(AddSkillPoint);

        playerLevel = localLevel;

        if (playerLevel != null)
            playerLevel.OnLevelUp.AddListener(AddSkillPoint);
    }

    private void OnEnable()
    {
        if (playerLevel != null)
            playerLevel.OnLevelUp.AddListener(AddSkillPoint);
    }

    private void OnDisable()
    {
        if (playerLevel != null)
            playerLevel.OnLevelUp.RemoveListener(AddSkillPoint);
    }

    public void AddSkillPoint()
    {
        currentskillPoint++;
        UpdateSkillPointText();
        OnSkillTreeChanged?.Invoke();
    }

    public bool CanUnLock(Skill skill)
    {
        if (currentskillPoint < skill.skillPointCost) return false;
        foreach (Skill prereq in skill.skillrequire)
            if (!unlockSkills.Contains(prereq)) return false;
        if (unlockSkills.Contains(skill)) return false;
        return true;
    }

    public bool Unlock(Skill skill, SkillBook skillBook)
    {
        if (!CanUnLock(skill))
        {
            Debug.Log("Can not Unlock skill");
            return false;
        }

        currentskillPoint -= skill.skillPointCost;
        UpdateSkillPointText();
        unlockSkills.Add(skill);
        skillBook.skillsSet.Add(skill);
        Debug.Log(skill.name + " unlock!");

        // ✅ ใส่ icon สกิลลงปุ่มช่องถัดไปใน UI
        UICanvasControllerInput.RegisterSkillToNextSlot(skill);

        OnSkillTreeChanged?.Invoke();
        return true;
    }

    public void UpdateSkillPointText()
    {
        skillPointText.text = "Skill Point : " + currentskillPoint;
    }

    public void ActiveSkillTreeUI()
    {
        isActive = !isActive;
        skillTreeUI.SetActive(isActive);
    }
}
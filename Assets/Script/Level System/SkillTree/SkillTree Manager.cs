using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public HashSet<Skill> unlockSkills = new HashSet<Skill>();
    public int currentskillPoint;

    public PlayerLevel playerLevel;
    public static SkillTreeManager instance;
    public System.Action OnSkillTreeChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        instance = this;
        playerLevel = FindAnyObjectByType<PlayerLevel>();
      
    }

    private void OnEnable()
    {
        if (playerLevel != null)
        {
            playerLevel.OnLevelUp.AddListener(AddSkillPoint);
        }
    }

    private void OnDisable()
    {
        playerLevel.OnLevelUp.RemoveListener(AddSkillPoint);
    }


    public void AddSkillPoint()
    {
        currentskillPoint++;
        OnSkillTreeChanged?.Invoke();
    }

    public bool CanUnLock(Skill skill)
    {
        if (currentskillPoint < skill.skillPointCost)
        {
            return false;
        }

        foreach (Skill prereq in skill.skillrequire)
        {
            if (!unlockSkills.Contains(prereq))
            {
                return false;
            }
        }

        if (unlockSkills.Contains(skill))
        {
            return false; 
        }

        return true;
    }

    public bool Unlock(Skill skill ,SkillBook skillBook)
    {
        if (!CanUnLock(skill))
        {
            Debug.Log("Can not Unlock skill");
            return false;
        }

        currentskillPoint -= skill.skillPointCost;
        unlockSkills.Add(skill);
        skillBook.skillsSet.Add(skill);
        Debug.Log(skill.name + " unlock!");
        OnSkillTreeChanged?.Invoke();
        return true;
    }

}

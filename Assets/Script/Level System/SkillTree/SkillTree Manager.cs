using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public HashSet<Skill> unlockSkills = new HashSet<Skill>();
    public int currentskillPoint;

    public PlayerLevel playerLevel;
    public static SkillTreeManager instance;
    public System.Action OnSkillTreeChanged;

    public GameObject skillTreeUI;
    private bool isActive = false;
    public TMP_Text skillPointText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        instance = this;
        playerLevel = FindFirstObjectByType<PlayerLevel>();
        UpdateSkillPointText();
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
        UpdateSkillPointText();
        OnSkillTreeChanged?.Invoke();
    }

    public bool CanUnLock(Skill skill)
    {
        if (currentskillPoint < skill.skillPointCost) //skillpoint
        {
            return false;
        }

        foreach (Skill prereq in skill.skillrequire) //skill ก่อนหน้า
        {
            if (!unlockSkills.Contains(prereq))
            {
                return false;
            }
        }

        if (unlockSkills.Contains(skill)) //unlock 
        {
            return false;
        }

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
        skillBook.skillsSet.Add(skill); //add skillbook
        skill.ResetCooldown();
        Debug.Log(skill.name + " unlock!");
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

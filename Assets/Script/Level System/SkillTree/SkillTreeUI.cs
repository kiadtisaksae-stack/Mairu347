using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    public Skill skill;
    public SkillBook skillBook;

    [Header("UI Components")]
    public Button skillButton;
    public Image skillIcon;
    public Image borderIcon;


    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color unlockableColor = Color.white;                 
    public Color unlockedColor = new Color(1f, 0.8f, 0f, 1f);   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        skillButton = GetComponent<Button>();
        skillIcon = GetComponent<Image>();
        skillButton.onClick.AddListener(OnSkillClicked);
        skillBook = FindFirstObjectByType<SkillBook>();
        UpdateVisual();
        SkillTreeManager.instance.OnSkillTreeChanged += UpdateVisual;
   
        //ใช้ไปก่อน
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSkillClicked()
    {
        SkillTreeManager.instance.Unlock(skill , skillBook);
    }

    public void UpdateVisual()
    {
        if (skill == null || SkillTreeManager.instance == null) return;

        skillIcon.sprite = skill.skillIcon;
        if (SkillTreeManager.instance.unlockSkills.Contains(skill)) //unlock
        {
            skillIcon.color = Color.white;
            if (borderIcon != null) borderIcon.color = unlockedColor;
            skillButton.interactable = false;
        }
        else if (SkillTreeManager.instance.CanUnLock(skill)) //canunlock?
        {
            skillIcon.color = Color.white;
            if (borderIcon != null) borderIcon.color = unlockableColor;
            skillButton.interactable = true;
        }
        else
        {
            skillIcon.color = lockedColor; //unlock
            if (borderIcon != null) borderIcon.color = lockedColor;
            skillButton.interactable = false;
        }
    }

    private void OnDestroy()
    {
        if (SkillTreeManager.instance != null)
        {
            SkillTreeManager.instance.OnSkillTreeChanged -= UpdateVisual;
        }
    }
}

using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public int currentSkillPoint;
    private PlayerLevel playerLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void OnEnable()
    {
        playerLevel.OnLevelUp.AddListener(AddSkillPoint);
    }


    private void OnDisable()
    {
        playerLevel.OnLevelUp.RemoveListener(AddSkillPoint);
    }

    public bool CanUnLock(Skill skill)
    {
        return true;
    }


    // Update is called once per frame

    public void AddSkillPoint()
    {
        currentSkillPoint++; 
    }

    void Update()
    {
        
    }
}

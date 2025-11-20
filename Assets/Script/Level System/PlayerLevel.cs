using UnityEngine;
using UnityEngine.Events;

public class PlayerLevel : MonoBehaviour
{
    public int currentXp;
    public int currentLevel;
    public int xpToNextLevel;

    public int baseXPRequirement = 100;
    public float xpMultiplierPerLevel = 1.5f;

    [Header("Events")]
    public UnityEvent OnLevelUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xpToNextLevel = baseXPRequirement;
        GameManager.Instance.UpdateLevel(currentLevel);
        GameManager.Instance.UpdateXpBar(currentXp, xpToNextLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddExperience(int XpAmount)
    {
        currentXp += XpAmount;
        GameManager.Instance.UpdateXpBar(currentXp , xpToNextLevel);
        while (currentXp >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        currentLevel++;
        currentXp -= xpToNextLevel;
        CalculateNextLevelXP();
        GameManager.Instance.UpdateXpBar(currentXp, xpToNextLevel);
        GameManager.Instance.UpdateLevel(currentLevel);
        Debug.Log("Level Up");
        if (OnLevelUp != null)
        {
            OnLevelUp?.Invoke();
        }
    }

    private void CalculateNextLevelXP()
    {
        xpToNextLevel = (int)(baseXPRequirement * Mathf.Pow(currentLevel, xpMultiplierPerLevel));
    }
}

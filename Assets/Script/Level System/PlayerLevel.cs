using UnityEngine;
using UnityEngine.Events;

public class PlayerLevel : MonoBehaviour
{
    public int currentXp;
    public int currentLevel = 1;
    public int xpToNextLevel;

    public int baseXPRequirement = 100;
    public float xpMultiplierPerLevel = 1.5f;

    [Header("Events")]
    public UnityEvent OnLevelUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xpToNextLevel = baseXPRequirement;
        GameManager.Instance.UpdateXpbar(currentXp, xpToNextLevel);
        GameManager.Instance.UpdateLevel(currentLevel);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddExperience(int XpAmount)
    {
        currentXp += XpAmount;
        GameManager.Instance.UpdateXpbar(currentXp , xpToNextLevel);
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
        Debug.Log("Level Up");
        GameManager.Instance.UpdateLevel(currentLevel);
        OnLevelUp?.Invoke();
    }

    private void CalculateNextLevelXP()
    {
        xpToNextLevel = (int)(baseXPRequirement * Mathf.Pow(currentLevel, xpMultiplierPerLevel));
        GameManager.Instance.UpdateXpbar(currentXp, xpToNextLevel);
    }
}

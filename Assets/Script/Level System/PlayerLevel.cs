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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddExperience(int XpAmount)
    {
        currentXp += XpAmount;
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
        if (OnLevelUp != null)
        {
            OnLevelUp.Invoke();
        }
    }

    private void CalculateNextLevelXP()
    {
        xpToNextLevel = (int)(baseXPRequirement * Mathf.Pow(currentLevel, xpMultiplierPerLevel));
    }
}

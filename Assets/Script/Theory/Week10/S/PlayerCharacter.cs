using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public int Health;
    public string PlayerName;
    public float PositionX;
    public float PositionY;

    // 1. หน้าที่เกี่ยวกับการเคลื่อนที่
    public void Move(float deltaX, float deltaY)
    {
        PositionX += deltaX;
        PositionY += deltaY;
        Debug.Log($"{PlayerName} moved to ({PositionX}, {PositionY})");
    }

    // 2. หน้าที่เกี่ยวกับการต่อสู้
    public void TakeDamage(int amount)
    {
        Health -= amount;
        Debug.Log($"{PlayerName} took {amount} damage. Health: {Health}");
        if (Health <= 0)
        {
            Debug.Log($"{PlayerName} has been defeated!");
        }
    }
    // 3. หน้าที่เกี่ยวกับการบันทึก/โหลดเกม
    public void SaveProgress()
    {
        Debug.Log($"Saving game for {PlayerName} to local file...");
        // Logic for saving player data to a file
    }
}

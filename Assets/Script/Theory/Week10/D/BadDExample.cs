using System;
using UnityEngine;

public class BadDExample : MonoBehaviour
{
    private GoblinEnemy _enemy;

    public BadDExample()
    {
        // GameManager สร้างและพึ่งพา GoblinEnemy โดยตรง (Tight Coupling)
        _enemy = new GoblinEnemy();
    }

    public void StartWave()
    {
        Debug.Log("--- Game Manager Starts Wave ---");
        _enemy.Spawn();
        _enemy.PerformAction();
    }
}
// 1. โมดูลระดับต่ำ: ศัตรูแบบ Concrete
public class GoblinEnemy
{
    public void Spawn()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Goblin] A new Goblin has spawned at (5, 5).");
        Console.ResetColor();
    }

    public void PerformAction()
    {
        Console.WriteLine("[Goblin] Goblin attacks the player!");
    }
}

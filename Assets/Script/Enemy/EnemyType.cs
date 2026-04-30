using System.Collections.Generic;
using UnityEngine;

public enum EnemyAIType
{
    Melee,
    Ranged,
    Boss,
    Passive
}

[CreateAssetMenu(fileName = "EnemyType", menuName = "Game/Enemy Config")]
public class EnemyType : ScriptableObject
{
    [Header("Identity")]
    public string enemyName;
    public int enemyId;
    public GameObject enemyPrefab;

    [Header("Stats")]
    public int initialMaxHealth = 100;
    public int damage;
    public int defence;
    public float movementSpeed;

    [Header("AI Behavior")]
    public EnemyAIType aiType = EnemyAIType.Melee;
    public float searchRadius = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("XP")]
    public int experience;
    public float xpShareRadius = 15f;

    [Header("Drop Items")]
    public List<DropEntry> dropTable = new List<DropEntry>();
    public int dropCount = 1;

    [System.Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Range(0, 100)] public int weight = 33;
    }
}
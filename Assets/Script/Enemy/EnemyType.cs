using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Game/EnemyType")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public int enemyId;

    [Header("Status")]
    public int _initialMaxHealth = 100;
    public int Damage;
    public int Defence;
    public float movementSpeed;
    public int experience;

    [Header("Drop Items")]
    public List<DropEntry> dropTable = new List<DropEntry>();

    // จำนวนไอเทมที่ drop ต่อครั้ง
    public int dropCount = 1;

    [System.Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Range(0, 100)] public int weight = 33; // น้ำหนักโอกาส drop
    }
}
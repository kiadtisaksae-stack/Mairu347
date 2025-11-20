using UnityEngine;

public enum QuestType
{
    CollectItem,
    KillEnemy,
    TalkToNPC,
    ReachLocation
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questName;
    public string description;
    public QuestType questType;

    [Header("Requirements")]
    public int requestCount;
    public int currentCount;

    [Header("Target Configuration")]
    // สำหรับเควสเก็บไอเทม
    public ItemSO targetItem;
    public int targetItemId;

    // สำหรับเควสฆ่ามอนสเตอร์
    public EnemyType targetEnemyType;
    public int targetEnemyId;

    [Header("Rewards")]
    public ItemSO[] rewardItems;
    public int rewardExp;
    public int rewardGold;

    [Header("Quest Chain")]
    public QuestData nextQuest;
}
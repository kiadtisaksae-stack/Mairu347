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
    public string CompletedText;
    public QuestType questType;

    [Header("Requirements")]
    public int requestCount;
    public int currentCount;

    // ✅ ถ้า true = ทำเสร็จแล้วต้องกลับมาส่ง NPC ก่อนรับรางวัล
    // ถ้า false = เสร็จแล้วรับรางวัลทันที
    [Header("Quest Delivery")]
    public bool requireDelivery = false;

    [Header("Target Configuration")]
    public ItemSO targetItem;
    public int targetItemId;
    public EnemyType targetEnemyType;
    public int targetEnemyId;

    [Header("Rewards")]
    public ItemSO[] rewardItems;
    public int rewardExp;
    public int rewardGold;

    [Header("Quest Chain")]
    public QuestData nextQuest;
}
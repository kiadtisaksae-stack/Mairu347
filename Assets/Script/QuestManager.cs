using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Active Quests")]
    public List<QuestData> activeQuests = new List<QuestData>();

    [Header("Completed Quests")]
    public List<QuestData> completedQuests = new List<QuestData>();

    // Events สำหรับแจ้งเตือน UI
    public UnityEvent<QuestData> OnQuestStarted;
    public UnityEvent<QuestData> OnQuestProgressUpdated;
    public UnityEvent<QuestData> OnQuestCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // เริ่มเควส
    public void StartQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} อยู่ในระบบแล้ว!");
            return;
        }

        quest.currentCount = 0;
        activeQuests.Add(quest);

        Debug.Log($"🎯 เริ่มเควส: {quest.questName}");
        OnQuestStarted?.Invoke(quest);
    }

    // ตรวจสอบเมื่อผู้เล่นเก็บไอเทม
    public void OnItemCollected(ItemSO collectedItem)
    {
        foreach (QuestData quest in activeQuests)
        {
            if (quest.questType == QuestType.CollectItem &&
                (quest.targetItem == collectedItem || quest.targetItemId == collectedItem.id))
            {
                AddProgress(quest, 1);
            }
        }
    }

    // ตรวจสอบเมื่อผู้เล่นฆ่ามอนสเตอร์

    // เพิ่มความคืบหน้า
    public void AddProgress(QuestData quest, int amount = 1)
    {
        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} ยังไม่ได้เริ่ม!");
            return;
        }

        quest.currentCount += amount;
        quest.currentCount = Mathf.Clamp(quest.currentCount, 0, quest.requestCount);

        Debug.Log($"📊 เควส {quest.questName}: {quest.currentCount}/{quest.requestCount}");
        OnQuestProgressUpdated?.Invoke(quest);

        CheckProgress(quest);
    }

    // ตรวจสอบครบ
    private void CheckProgress(QuestData quest)
    {
        if (quest.currentCount >= quest.requestCount)
        {
            CompleteQuest(quest);
        }
    }

    // เควสสำเร็จ
    private void CompleteQuest(QuestData quest)
    {
        Debug.Log($"✅ เควสสำเร็จ: {quest.questName}");

        // ให้รางวัล
        GiveRewards(quest);

        // ย้ายไปยังรายการเควสที่สำเร็จ
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        OnQuestCompleted?.Invoke(quest);

        // เริ่มเควสถัดไป (ถ้ามี)
        if (quest.nextQuest != null)
        {
            StartQuest(quest.nextQuest);
        }
    }

    // ให้รางวัล
    private void GiveRewards(QuestData quest)
    {
        // ให้ EXP
        if (quest.rewardExp > 0)
        {
            PlayerLevel playerLevel = FindObjectOfType<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.AddExperience(quest.rewardExp);
                Debug.Log($"🎁 ได้รับ EXP: {quest.rewardExp}");
            }
        }

        // ให้ไอเทม
        if (quest.rewardItems != null && quest.rewardItems.Length > 0)
        {
            InventoryCanvas inventory = FindObjectOfType<InventoryCanvas>();
            if (inventory != null)
            {
                foreach (ItemSO rewardItem in quest.rewardItems)
                {
                    inventory.AddItem(rewardItem, 1);
                    Debug.Log($"🎁 ได้รับไอเทม: {rewardItem.itemName}");
                }
            }
        }
    }

    // ตรวจสอบสถานะเควส
    public bool IsQuestActive(QuestData quest)
    {
        return activeQuests.Contains(quest);
    }

    public bool IsQuestCompleted(QuestData quest)
    {
        return completedQuests.Contains(quest);
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Active Quests")]
    public List<QuestData> activeQuests = new List<QuestData>();

    [Header("Completed Quests")]
    public List<QuestData> completedQuests = new List<QuestData>();
    public TextMeshProUGUI giveRewardText;

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
    void Start()
    {
        giveRewardText.gameObject.SetActive(false);
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
        foreach (QuestData quest in activeQuests.ToArray())
        {
            if (quest.questType == QuestType.CollectItem &&
                (quest.targetItem == collectedItem))
            {
                AddProgress(quest, 1);
            }
        }
    }

    // ตรวจสอบเมื่อผู้เล่นฆ่ามอนสเตอร์
    public void OnEnemyKilled(EnemyType killedEnemy)
    {
        foreach (QuestData quest in activeQuests.ToArray())
        {
            if (quest.questType == QuestType.KillEnemy &&
                (quest.targetEnemyType == killedEnemy || quest.targetEnemyId == killedEnemy.enemyId))
            {
                AddProgress(quest, 1);
            }
        }
    }

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

    }

    // ตรวจสอบครบ
    public void CheckProgress(QuestData quest)
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
                giveRewardText.text = " Get Exp = " + quest.rewardExp;
                StartCoroutine(CloseAfterTime(giveRewardText.gameObject, 3f)); 
                
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
                    giveRewardText.text = " Get Item = " + rewardItem.itemName;
                    StartCoroutine(CloseAfterTime(giveRewardText.gameObject, 3f)); 
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
    public IEnumerator CloseAfterTime(GameObject obj, float delay)
    {
        giveRewardText.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);   // ปิด object
    }
}
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Active Quests")]
    public List<QuestData> activeQuests = new List<QuestData>(); // เควสที่กำลังทำอยู่
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
 

    // เริ่มเควส (ส่ง SO ตรงมาเลย)
    public void StartQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} อยู่ใน active list แล้ว!");
            return;
        }

        quest.CurrentCount = 0;
        activeQuests.Add(quest);

        Debug.Log($"🎯 เริ่มเควส: {quest.questName} / ต้องทำ {quest.requestCount} ครั้ง");
    }

    // เพิ่มความคืบหน้า
    public void AddProgress(QuestData quest, int amount = 1)
    {
        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} ยังไม่ได้เริ่ม!");
            return;
        }

        quest.CurrentCount += amount;

        CheckProgress(quest);
    }

    // ตรวจสอบครบ
    private void CheckProgress(QuestData quest)
    {
        if (quest.CurrentCount >= quest.requestCount)
        {
            CompleteQuest(quest);
        }
    }

    // เควสสำเร็จ
    private void CompleteQuest(QuestData quest)
    {
        Debug.Log($"✅ เควสสำเร็จแล้ว: {quest.questName}");
        activeQuests.Remove(quest);
    }
}

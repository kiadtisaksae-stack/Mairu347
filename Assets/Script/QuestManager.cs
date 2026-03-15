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

    public UnityEvent<QuestData> OnQuestStarted;
    public UnityEvent<QuestData> OnQuestProgressUpdated;
    public UnityEvent<QuestData> OnQuestCompleted;

    private Dictionary<QuestData, int> _questProgress = new Dictionary<QuestData, int>();

    // ✅ เควสที่ทำครบแล้ว รอกลับไปส่ง NPC
    private HashSet<QuestData> _pendingDelivery = new HashSet<QuestData>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        giveRewardText.gameObject.SetActive(false);
    }

    public void StartQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} อยู่ในระบบแล้ว!");
            return;
        }

        _questProgress[quest] = 0;
        activeQuests.Add(quest);
        GameManager.Instance.UpdateQuestUI(quest);
        Debug.Log($"🎯 เริ่มเควส: {quest.questName}");
        OnQuestStarted?.Invoke(quest);
    }

    public int GetProgress(QuestData quest)
        => _questProgress.ContainsKey(quest) ? _questProgress[quest] : 0;

    // ✅ DialogueManager ใช้เช็คว่าควรแสดงปุ่ม sendQuestBtn ไหม
    public bool IsPendingDelivery(QuestData quest)
        => _pendingDelivery.Contains(quest);

    public void OnItemCollected(ItemSO collectedItem)
    {
        foreach (QuestData quest in activeQuests.ToArray())
        {
            if (quest.questType == QuestType.CollectItem && quest.targetItem == collectedItem)
            {
                AddProgress(quest, 1);
                GameManager.Instance.UpdateQuestUI(quest);
            }
        }
    }

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

    public void AddProgress(QuestData quest, int amount = 1)
    {
        if (!activeQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} ยังไม่ได้เริ่ม!");
            return;
        }

        int current = Mathf.Clamp(GetProgress(quest) + amount, 0, quest.requestCount);
        _questProgress[quest] = current;

        Debug.Log($"📊 {quest.questName}: {current}/{quest.requestCount}");
        OnQuestProgressUpdated?.Invoke(quest);

        CheckProgress(quest);
    }

    public void CheckProgress(QuestData quest)
    {
        if (GetProgress(quest) < quest.requestCount) return;

        if (quest.requireDelivery)
        {
            // ทำครบแล้ว — รอส่ง NPC
            if (!_pendingDelivery.Contains(quest))
            {
                _pendingDelivery.Add(quest);
                Debug.Log($"📬 {quest.questName} ครบแล้ว — กลับไปส่ง NPC เพื่อรับรางวัล");
                OnQuestProgressUpdated?.Invoke(quest);
            }
        }
        else
        {
            // ไม่ต้องส่ง — รับรางวัลทันที
            CompleteQuest(quest);
        }
    }

    // ✅ DialogueManager.SendQuest() เรียกตรงนี้
    // คืน true = ส่งสำเร็จ, false = ยังไม่ครบหรือไม่ต้องส่ง
    public bool TryDeliverQuest(QuestData quest)
    {
        if (!_pendingDelivery.Contains(quest))
        {
            Debug.Log($"[QuestManager] {quest.questName} ยังไม่ครบ หรือไม่ต้อง delivery");
            return false;
        }

        _pendingDelivery.Remove(quest);
        CompleteQuest(quest);
        return true;
    }

    private void CompleteQuest(QuestData quest)
    {
        Debug.Log($"✅ เควสสำเร็จ: {quest.questName}");
        GiveRewards(quest);
        GameManager.Instance.ClearQuest(quest);
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        _questProgress.Remove(quest);
        _pendingDelivery.Remove(quest);
        OnQuestCompleted?.Invoke(quest);

        if (quest.nextQuest != null)
            StartQuest(quest.nextQuest);
    }

    public void SyncProgressToQuestData(QuestData quest)
    {
        if (_questProgress.ContainsKey(quest))
            quest.currentCount = _questProgress[quest];
    }

    private void GiveRewards(QuestData quest)
    {
        PlayerLevel playerLevel = FindObjectOfType<PlayerLevel>();
        InventoryCanvas inventory = FindObjectOfType<InventoryCanvas>();

        if (quest.rewardExp > 0 && playerLevel != null)
            playerLevel.AddExperience(quest.rewardExp);

        if (quest.rewardItems != null && inventory != null)
            foreach (ItemSO item in quest.rewardItems)
                inventory.AddItem(item, 1);

        string msg = "";
        if (quest.rewardExp > 0) msg += $"⭐ EXP: {quest.rewardExp}\n";
        if (quest.rewardItems != null)
            foreach (var item in quest.rewardItems)
                msg += $"🎁 {item.itemName}\n";

        if (msg != "")
        {
            giveRewardText.text = msg.TrimEnd();
            StartCoroutine(CloseAfterTime(giveRewardText.gameObject, 3f));
        }
    }

    public bool IsQuestActive(QuestData quest) => activeQuests.Contains(quest);
    public bool IsQuestCompleted(QuestData quest) => completedQuests.Contains(quest);

    public IEnumerator CloseAfterTime(GameObject obj, float delay)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }
}
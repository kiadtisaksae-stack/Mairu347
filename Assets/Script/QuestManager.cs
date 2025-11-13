<<<<<<< Updated upstream
﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

// สร้าง Enum สำหรับประเภทของศัตรู
public enum EnemyType
{
    EnemyRange,
    EnemyMovetoPlayer,
    All
}

// โครงสร้างสำหรับเป้าหมายของเควสย่อย
[System.Serializable]
public class QuestObjective
{
    public EnemyType enemyType;
    public string objectiveDescription;
    public int requiredCount;
    public int currentCount;
    public bool isCompleted => currentCount >= requiredCount;

    public void Progress(EnemyType type)
    {
        // เควสที่ต้องการศัตรูทุกประเภท หรือตรงกับประเภทที่กำหนด
        if (enemyType == EnemyType.All || enemyType == type)
        {
            if (currentCount < requiredCount)
            {
                currentCount++;
                Debug.Log($"ความคืบหน้าเควส: {objectiveDescription} ({currentCount}/{requiredCount})");
            }
        }
    }
}

// โครงสร้างสำหรับเควสหลัก (รวมเป้าหมายย่อย)
[System.Serializable]
public class QuestData
{
    public string questName;
    public List<QuestObjective> objectives;
    public bool isCompleted => objectives.All(obj => obj.isCompleted);
    public bool isActive = false;
}


public class QuestManager : MonoBehaviour
{
    // ใช้ List ในการเก็บเควสที่ใช้งานอยู่
    public List<QuestData> activeQuests = new List<QuestData>();
    private InputSystem_Actions inputActions;
=======
﻿using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
>>>>>>> Stashed changes

    [Header("Active Quests")]
    public List<QuestData> activeQuests = new List<QuestData>(); // เควสที่กำลังทำอยู่
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
<<<<<<< Updated upstream
    };

    // 🎯 เควสที่ 2: เก็บไอเทม
    private QuestData questCollectItems = new QuestData
    {
        questName = "ภารกิจเก็บเกี่ยว",
        objectives = new List<QuestObjective>
        {
            new QuestObjective { enemyType = (EnemyType)(-1), objectiveDescription = "เก็บ CollectableItem 2 ชิ้น", requiredCount = 2, currentCount = 0 } // ใช้ -1 หรือประเภทพิเศษสำหรับ Item
        }
    };
    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Q.performed += ctx => AcceptQuests();
        inputActions.Player.T.performed += ctx => SubmitQuests();


=======
        Instance = this;
        DontDestroyOnLoad(gameObject);
>>>>>>> Stashed changes
    }
 

    // เริ่มเควส (ส่ง SO ตรงมาเลย)
    public void StartQuest(QuestData quest)
    {
<<<<<<< Updated upstream
        //// 1. กด Q เพื่อรับเควส
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    AcceptQuests();
        //}

        //// 2. กด T เพื่อส่งเควส (ตรวจสอบ)
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    SubmitQuests();
        //}
    }

    // ----------------------------------------------------
    // --- 1. การรับเควส ---
    // ----------------------------------------------------

    private void AcceptQuests()
    {
        if (!questKillEnemies.isActive)
        {
            // รีเซ็ตสถานะและเพิ่มเข้า List
            questKillEnemies.objectives.ForEach(o => o.currentCount = 0);
            activeQuests.Add(questKillEnemies);
            questKillEnemies.isActive = true;
            Debug.Log("📢 **รับเควสใหม่: " + questKillEnemies.questName + "**");
        }

        if (!questCollectItems.isActive)
        {
            questCollectItems.objectives.ForEach(o => o.currentCount = 0);
            activeQuests.Add(questCollectItems);
            questCollectItems.isActive = true;
            Debug.Log("📢 **รับเควสใหม่: " + questCollectItems.questName + "**");
        }
    }

    // ----------------------------------------------------
    // --- 2. การติดตามความคืบหน้า (Kill) ---
    // ----------------------------------------------------

    // ฟังก์ชันนี้ถูกเรียกเมื่อศัตรูตาย (จากคลาส Enemy/Character)
    public void TrackEnemyKill(EnemyType type)
    {
        foreach (var quest in activeQuests.Where(q => q.questName == "การทดสอบนักรบ"))
        {
            foreach (var objective in quest.objectives)
            {
                objective.Progress(type);
            }
        }
    }

    // ----------------------------------------------------
    // --- 3. การติดตามความคืบหน้า (Collect) ---
    // ----------------------------------------------------

    // ฟังก์ชันนี้ถูกเรียกเมื่อเก็บ CollectableItem (จากคลาส CollectableItem)
    public void TrackCollectItem(string itemName)
    {
        // สมมติว่าทุก CollectableItem นับเป็นความคืบหน้าเดียวกัน
        foreach (var quest in activeQuests.Where(q => q.questName == "ภารกิจเก็บเกี่ยว"))
        {
            foreach (var objective in quest.objectives)
            {
                // ตรวจสอบจาก RequiredCount ได้เลย
                if (objective.requiredCount > 0)
                {
                    objective.currentCount++;
                    Debug.Log($"ความคืบหน้าเควส: {objective.objectiveDescription} ({objective.currentCount}/{objective.requiredCount})");
                }
            }
        }
    }

    // ----------------------------------------------------
    // --- 4. การส่งเควส ---
    // ----------------------------------------------------

    private void SubmitQuests()
    {
        if (activeQuests.Count == 0)
        {
            Debug.LogWarning("❌ ไม่มีเควสที่ใช้งานอยู่ให้ส่ง!");
=======
        if (activeQuests.Contains(quest))
        {
            Debug.LogWarning($"❌ เควส {quest.questName} อยู่ใน active list แล้ว!");
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
}
=======

    // เควสสำเร็จ
    private void CompleteQuest(QuestData quest)
    {
        Debug.Log($"✅ เควสสำเร็จแล้ว: {quest.questName}");
        activeQuests.Remove(quest);
    }
}
>>>>>>> Stashed changes

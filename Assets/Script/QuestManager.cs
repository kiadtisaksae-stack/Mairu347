using UnityEngine;
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

    // 🎯 เควสที่ 1: ฆ่าศัตรู
    private QuestData questKillEnemies = new QuestData
    {
        questName = "การทดสอบนักรบ",
        objectives = new List<QuestObjective>
        {
            new QuestObjective { enemyType = EnemyType.EnemyRange, objectiveDescription = "ตี Enemy Range 2 ตัว", requiredCount = 2, currentCount = 0 },
            new QuestObjective { enemyType = EnemyType.EnemyMovetoPlayer, objectiveDescription = "ตี Enemy Move 2 ตัว", requiredCount = 2, currentCount = 0 },
            new QuestObjective { enemyType = EnemyType.All, objectiveDescription = "ตีศัตรูรวม 5 ตัว", requiredCount = 5, currentCount = 0 }
        }
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


    }

    void Update()
    {
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
            return;
        }

        List<QuestData> questsToComplete = new List<QuestData>();

        foreach (var quest in activeQuests.ToList()) // ใช้ ToList() เพื่อป้องกันการแก้ไข List ระหว่างวนซ้ำ
        {
            if (quest.isCompleted)
            {
                Debug.Log($"✅ **ส่งเควสสำเร็จ: {quest.questName}**");
                questsToComplete.Add(quest);
                quest.isActive = false;
                // [TODO] เพิ่มโค้ดให้รางวัล
            }
            else
            {
                // เงื่อนไข: ถ้าไม่ครบ ให้ Debug
                Debug.LogWarning($"⚠️ เควส **{quest.questName}** ยังไม่สำเร็จ:");
                foreach (var obj in quest.objectives.Where(o => !o.isCompleted))
                {
                    Debug.Log($" - ขาด: {obj.objectiveDescription} ({obj.currentCount}/{obj.requiredCount})");
                }
            }
        }

        // ลบเควสที่เสร็จสิ้นออกจากรายการ
        foreach (var completedQuest in questsToComplete)
        {
            activeQuests.Remove(completedQuest);
        }
    }
}
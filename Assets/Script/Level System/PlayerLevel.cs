using Unity.Netcode; // เพิ่มตัวนี้
using UnityEngine;
using UnityEngine.Events;

// เปลี่ยนจาก MonoBehaviour เป็น NetworkBehaviour
public class PlayerLevel : NetworkBehaviour
{
    // ใช้ NetworkVariable เพื่อให้ค่าซิงค์กันระหว่าง Server และ Client
    public NetworkVariable<int> currentXp = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentLevel = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public int xpToNextLevel;
    public int baseXPRequirement = 100;
    public float xpMultiplierPerLevel = 1.5f;

    [Header("Events")]
    public UnityEvent OnLevelUp;

    public override void OnNetworkSpawn()
    {
        // คำนวณ XP ที่ต้องใช้ ณ เลเวลปัจจุบัน
        CalculateNextLevelXP();

        // สมัครรับการแจ้งเตือนเมื่อค่าเปลี่ยน (เพื่ออัปเดต UI)
        currentXp.OnValueChanged += (oldVal, newVal) => UpdateUI();
        currentLevel.OnValueChanged += (oldVal, newVal) => {
            UpdateUI();
            if (newVal > oldVal) OnLevelUp?.Invoke();
        };

        if (IsOwner) UpdateUI();
    }

    private void UpdateUI()
    {
        if (!IsOwner) return;
        GameManager.Instance.UpdateXpbar(currentXp.Value, xpToNextLevel);
        GameManager.Instance.UpdateLevel(currentLevel.Value);
    }

    // การเพิ่ม XP ต้องทำที่ Server เท่านั้น
    public void AddExperience(int XpAmount)
    {
        if (!IsServer) return;

        currentXp.Value += XpAmount;
        while (currentXp.Value >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // ไม่ต้องเช็ค IsServer ซ้ำเพราะเรียกมาจาก AddExperience ที่เช็คแล้ว
        currentLevel.Value++;
        currentXp.Value -= xpToNextLevel;
        CalculateNextLevelXP();

        // เรียก ClientRpc หากต้องการให้มี Effect เล่นที่เครื่องคนอื่นด้วย
        // LevelUpClientRpc(); 
    }

    private void CalculateNextLevelXP()
    {
        // ใช้ .Value สำหรับ NetworkVariable
        xpToNextLevel = (int)(baseXPRequirement * Mathf.Pow(currentLevel.Value, xpMultiplierPerLevel));
    }
}
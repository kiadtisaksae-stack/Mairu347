using UnityEngine;
using Unity.Netcode;

public class PlayerDataNetwork : NetworkBehaviour
{
    public static PlayerDataNetwork Instance { get; private set; }

    [Header("Player Data Slots")]
    public PlayerData[] playerDataSlots; // 1 Data ต่อ 1 Player

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

    // ✅ บันทึกข้อมูล (เรียกจาก Player โดยตรง)
    public void SavePlayerData(ulong clientId, PlayerData data)
    {
        if (!IsServer) return;

        int slotIndex = (int)clientId;
        if (slotIndex < playerDataSlots.Length)
        {
            playerDataSlots[slotIndex] = data;
            Debug.Log($"💾 บันทึกข้อมูล Player {clientId}: HP={data.health}");
        }
    }

    // ✅ โหลดข้อมูล (เรียกจาก Player โดยตรง)
    public PlayerData LoadPlayerData(ulong clientId)
    {
        int slotIndex = (int)clientId;
        if (slotIndex < playerDataSlots.Length && playerDataSlots[slotIndex] != null)
        {
            Debug.Log($"📂 โหลดข้อมูล Player {clientId} เรียบร้อย");
            return playerDataSlots[slotIndex];
        }
        return null;
    }
}
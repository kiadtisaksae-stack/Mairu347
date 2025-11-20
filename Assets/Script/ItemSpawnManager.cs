using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;
using Random = UnityEngine.Random;

public class ItemSpawnManager : NetworkBehaviour
{
    // 💡 Singleton Instance
    public static ItemSpawnManager Instance { get; private set; }

    [Header("Item Spawn Settings")]
    [Tooltip("อาร์เรย์ของ Item Prefab ที่สามารถสุ่มสร้างได้")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Tooltip("จุด Spawn Item ที่ถูกวางใน Scene")]
    [SerializeField] private Transform[] itemSpawnPoints;

    // 💡 NetworkList: เก็บ ID ของ Item ที่ 'ถูกเก็บ' ไปแล้ว (สำหรับ Late Joiner Sync)
    private NetworkList<ulong> collectedItemIds = new NetworkList<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnInitialItems();
        }
        else
        {
            collectedItemIds.OnListChanged += HandleCollectedItemIdsChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            collectedItemIds.Dispose();
        }
        else
        {
            collectedItemIds.OnListChanged -= HandleCollectedItemIdsChanged;
        }
    }

    // ----------------------------------------------------
    // 🔄 Server Logic: การ Spawn Item
    // ----------------------------------------------------

    private void SpawnInitialItems()
    {
        if (!IsServer || itemPrefabs == null || itemPrefabs.Length == 0) return;

        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            // 1. สุ่มเลือก Item Prefab
            GameObject selectedPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            // 2. สร้างและ Spawn Item (ใช้ตำแหน่งของ SpawnPoint)
            GameObject itemObj = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkObject itemNetObj = itemObj.GetComponent<NetworkObject>();

            itemNetObj.Spawn();

            
        }
    }

    // ----------------------------------------------------
    // 📢 Server Logic: บันทึก Item ที่ถูกเก็บ
    // ----------------------------------------------------

    // Host/Server บันทึก ID Item ที่ถูกเก็บลงใน NetworkList
    public void HandleItemCollected(ulong collectedItemId)
    {
        if (!IsServer) return;

        if (!collectedItemIds.Contains(collectedItemId))
        {
            collectedItemIds.Add(collectedItemId);
            Debug.Log($"[SERVER] Item Collected: {collectedItemId}. Tracking for late joiners.");
        }
    }

    // ----------------------------------------------------
    // 💻 Client Logic: การตรวจสอบและทำลาย Object (สำหรับ Late Joiner)
    // ----------------------------------------------------

    private void HandleCollectedItemIdsChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return;

        // changeEvent.Value คือ NetworkObjectId ของ Item ที่ถูกเก็บ
        CheckAndDestroyObject(changeEvent.Value);
    }

    private void CheckAndDestroyObject(ulong networkId)
    {
        // ตรวจสอบว่า Netcode ได้ทำการ Spawn Object นี้ขึ้นมาบน Client แล้วหรือไม่
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject netObj))
        {
            // ถ้าถูกบันทึกว่าถูกเก็บไปแล้วบน Server ให้นำมันออกจาก Scene ของ Client ใหม่
            Destroy(netObj.gameObject);
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Destroyed collected Item ID: {networkId}");
        }
    }
}
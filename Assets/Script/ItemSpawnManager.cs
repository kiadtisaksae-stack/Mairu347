using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using Random = UnityEngine.Random;

public class ItemSpawnManager : NetworkBehaviour
{
    public static ItemSpawnManager Instance { get; private set; }

    [Header("Item Spawn Settings")]
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private Transform[] itemSpawnPoints;

    private NetworkList<ulong> collectedItemIds = new NetworkList<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        // ไม่ตั้ง Instance ใน Awake อีกต่อไป — ย้ายไป OnNetworkSpawn แล้ว
    }

    // แก้ปัญหา Static + NetworkBehaviour
    public override void OnNetworkSpawn()
    {
        // ✅ ตั้ง Instance ตอน Spawn
        Instance = this;

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
        // ✅ ล้าง Instance ตอน Despawn
        if (Instance == this)
            Instance = null;

        if (IsServer)
        {
            collectedItemIds.Dispose();
        }
        else
        {
            collectedItemIds.OnListChanged -= HandleCollectedItemIdsChanged;
        }
    }

    private void SpawnInitialItems()
    {
        if (!IsServer || itemPrefabs == null || itemPrefabs.Length == 0) return;

        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            GameObject selectedPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            GameObject itemObj = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkObject itemNetObj = itemObj.GetComponent<NetworkObject>();
            itemNetObj.Spawn();
        }
    }

    public void HandleItemCollected(ulong collectedItemId)
    {
        if (!IsServer) return;
        if (!collectedItemIds.Contains(collectedItemId))
        {
            collectedItemIds.Add(collectedItemId);
            Debug.Log($"[SERVER] Item Collected: {collectedItemId}.");
        }
    }

    private void HandleCollectedItemIdsChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return;
        CheckAndDestroyObject(changeEvent.Value);
    }

    private void CheckAndDestroyObject(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject netObj))
        {
            Destroy(netObj.gameObject);
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Destroyed collected Item ID: {networkId}");
        }
    }
}
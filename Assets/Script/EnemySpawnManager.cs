using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Collections;
using Random = UnityEngine.Random;

public class EnemySpawnManager : NetworkBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int minEnemiesPerPoint = 1;
    [SerializeField] private int maxEnemiesPerPoint = 3;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnCheckInterval = 10f;
    [SerializeField] private LayerMask enemyLayer;

    private float lastRespawnCheckTime = 0f;

    private NetworkList<ulong> destroyedEnemyIds = new NetworkList<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // แก้ปัญหา Static + NetworkBehaviour
    // ย้ายจาก Awake() → OnNetworkSpawn/Despawn
    // เพื่อให้ Instance ชี้ไปที่ object ที่ Active อยู่จริงเสมอ
    private void Awake()
    {
        // ไม่ตั้ง Instance ใน Awake อีกต่อไป
        // เพราะถ้า Despawn แล้ว Spawn ใหม่ Instance จะยังชี้ตัวเก่า
    }

    public override void OnNetworkSpawn()
    {
        // ✅ ตั้ง Instance ตอน Spawn — รับประกันว่าชี้ไปที่ตัวที่ Active อยู่
        Instance = this;

        if (IsServer)
        {
            SpawnAllDefinedPoints();
        }
        else
        {
            destroyedEnemyIds.OnListChanged += HandleDestroyedEnemyListChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        // ✅ ล้าง Instance ตอน Despawn — ป้องกัน stale reference
        if (Instance == this)
            Instance = null;

        if (IsServer)
        {
            destroyedEnemyIds.Dispose();
        }
        else
        {
            destroyedEnemyIds.OnListChanged -= HandleDestroyedEnemyListChanged;
        }
    }

    private void LateUpdate()
    {
        if (!IsServer) return;
        if (Time.time >= lastRespawnCheckTime + respawnCheckInterval)
        {
            CheckAndRespawnAllPoints();
            lastRespawnCheckTime = Time.time;
        }
    }

    private void SpawnEnemiesAtPoint(Transform spawnPoint, GameObject[] prefabsToUse)
    {
        if (!IsServer || prefabsToUse == null || prefabsToUse.Length == 0) return;

        int enemiesToSpawn = Random.Range(minEnemiesPerPoint, maxEnemiesPerPoint + 1);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            GameObject enemyObj = Instantiate(selectedPrefab, spawnPosition, spawnPoint.rotation);
            NetworkObject enemyNetObj = enemyObj.GetComponent<NetworkObject>();
            enemyNetObj.Spawn();

            if (enemyObj.TryGetComponent(out Character enemyCharacter))
            {
                enemyCharacter.OnDestory += HandleEnemyDestroyed;
            }
        }
    }

    private void SpawnAllDefinedPoints()
    {
        if (!IsServer) return;
        foreach (Transform spawnPoint in spawnPoints)
        {
            SpawnEnemiesAtPoint(spawnPoint, enemyPrefabs);
        }
    }

    public void OnTriggerSpawn(Transform[] pointsToUse, GameObject[] prefabsToUse)
    {
        if (!IsServer) return;
        if (pointsToUse == null || pointsToUse.Length == 0)
        {
            Debug.LogWarning("[SERVER] OnTriggerSpawn received no spawn points.");
            return;
        }
        foreach (Transform spawnPoint in pointsToUse)
        {
            SpawnEnemiesAtPoint(spawnPoint, prefabsToUse);
        }
    }

    private void CheckAndRespawnAllPoints()
    {
        if (!IsServer || enemyLayer == 0) return;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Collider[] hitColliders = Physics.OverlapSphere(spawnPoint.position, spawnRadius, enemyLayer);
            if (hitColliders.Length < 1)
            {
                Debug.Log($"[SERVER] Respawning at {spawnPoint.name}.");
                SpawnEnemiesAtPoint(spawnPoint, enemyPrefabs);
            }
        }
    }

    public void HandleEnemyDestroyed(Idestoryable destroyedObject)
    {
        if (!IsServer) return;
        if (destroyedObject is Character enemyCharacter)
        {
            ulong id = enemyCharacter.NetworkObjectId;
            if (!destroyedEnemyIds.Contains(id))
            {
                destroyedEnemyIds.Add(id);
                Debug.Log($"[SERVER] Enemy Destroyed: {id}.");
            }
        }
    }

    private void HandleDestroyedEnemyListChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return;
        CheckAndDestroyObject(changeEvent.Value);
    }

    private void CheckAndDestroyObject(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject netObj))
        {
            Destroy(netObj.gameObject);
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Destroyed Late Joiner Enemy ID: {networkId}");
        }
    }
}
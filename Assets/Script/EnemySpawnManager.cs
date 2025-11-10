using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections; 
using Random = UnityEngine.Random;

public class EnemySpawnManager : NetworkBehaviour
{
    // 💡 Singleton Instance: ใช้ในการเข้าถึงจากคลาสอื่น (เช่น SpawnTrigger หรือ Character)
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [Tooltip("อาร์เรย์ของ Prefab ศัตรูที่สามารถสุ่มสร้างได้")]
    [SerializeField] private GameObject[] enemyPrefabs; 
    
    [Tooltip("จำนวนศัตรูขั้นต่ำที่ต้องการสร้างต่อจุด Spawn")]
    [SerializeField] private int minEnemiesPerPoint = 1; 
    
    [Tooltip("จำนวนศัตรูสูงสุดที่ต้องการสร้างต่อจุด Spawn")]
    [SerializeField] private int maxEnemiesPerPoint = 3; 
    
    [Tooltip("รัศมีการสุ่มตำแหน่งการเกิดรอบจุด Spawn")]
    [SerializeField] private float spawnRadius = 2f; 
    
    [SerializeField] private Transform[] spawnPoints;

    [Header("Respawn Settings")]
    [Tooltip("ช่วงเวลาเป็นวินาทีในการตรวจสอบและ Respawn ศัตรู")]
    [SerializeField] private float respawnCheckInterval = 10f; 
    
    [Tooltip("LayerMask สำหรับระบุว่า GameObject ใดคือศัตรู (จำเป็นสำหรับการตรวจสอบ Respawn)")]
    [SerializeField] private LayerMask enemyLayer; 
    
    private float lastRespawnCheckTime = 0f; // ตัวจับเวลา Respawn

    // 💡 NetworkList: เก็บ ID ของศัตรูที่ 'ถูกทำลาย' ไปแล้ว (สำหรับ Late Joiner Sync)
    private NetworkList<ulong> destroyedEnemyIds = new NetworkList<ulong>(
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
            // Host/Server: เรียก Spawn ศัตรูเริ่มต้น
            SpawnInitialEnemies();
        }
        else
        {
            // Client ใหม่ (Late Joiner): สมัครรับ Event เพื่อซิงค์สถานะการทำลาย
            destroyedEnemyIds.OnListChanged += HandleDestroyedEnemyListChanged;
        }
    }
    public void OnTriggerSpawn()
    {
        if (IsServer)
        {
            SpawnInitialEnemies();
        }
    
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            destroyedEnemyIds.Dispose();
        }
        else
        {

            destroyedEnemyIds.OnListChanged -= HandleDestroyedEnemyListChanged;
        }
    }
    
    // ----------------------------------------------------
    // 🕒 Server Logic: ควบคุมจังหวะการ Respawn
    // ----------------------------------------------------

    private void LateUpdate()
    {
        // Host/Server ควบคุมจังหวะการตรวจสอบเท่านั้น
        if (!IsServer) return; 
        
        if (Time.time >= lastRespawnCheckTime + respawnCheckInterval)
        {
            CheckAndRespawnAllPoints();
            lastRespawnCheckTime = Time.time;
        }
    }

    // ----------------------------------------------------
    // 🎯 Shared Logic: การ Spawn ศัตรูที่จุดเดียว
    // ----------------------------------------------------
    
    private void SpawnEnemiesAtPoint(Transform spawnPoint)
    {
        if (!IsServer || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 1. สุ่มจำนวนศัตรูที่จะเกิด ณ จุดนี้
        int enemiesToSpawn = Random.Range(minEnemiesPerPoint, maxEnemiesPerPoint + 1);
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // 2. สุ่มเลือก Prefab ศัตรู
            GameObject selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // 3. สุ่มตำแหน่งการเกิดรอบจุด Spawn
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y); 

            // 4. สร้างและ Spawn ศัตรู
            GameObject enemyObj = Instantiate(selectedPrefab, spawnPosition, spawnPoint.rotation);
            NetworkObject enemyNetObj = enemyObj.GetComponent<NetworkObject>();
            
            enemyNetObj.Spawn();

            // 5. ติดตาม Event การตาย (เพื่อบันทึกสถานะลงใน NetworkList)
            if (enemyObj.TryGetComponent(out Character enemyCharacter))
            {
                enemyCharacter.OnDestory += HandleEnemyDestroyed;
            }
        }
    }

    // ----------------------------------------------------
    // 🔄 Server Logic: การสร้างศัตรูเริ่มต้น
    // ----------------------------------------------------

    private void SpawnInitialEnemies()
    {
        if (!IsServer) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            SpawnEnemiesAtPoint(spawnPoint);
        }
    }

    // ----------------------------------------------------
    // ♻️ Server Logic: การตรวจสอบและ Respawn
    // ----------------------------------------------------

    private void CheckAndRespawnAllPoints()
    {
        if (!IsServer || enemyLayer == 0) return; 

        foreach (Transform spawnPoint in spawnPoints)
        {
            // 1. ตรวจสอบจำนวนศัตรูที่มีอยู่ในรัศมี
            Collider[] hitColliders = Physics.OverlapSphere(
                spawnPoint.position, 
                spawnRadius, 
                enemyLayer
            );

            int currentEnemyCount = hitColliders.Length;

            // 2. เงื่อนไข Respawn: ถ้าจำนวนศัตรูในบริเวณนั้นน้อยกว่า 1 (คือ 0)
            if (currentEnemyCount < 1) 
            {
                Debug.Log($"[SERVER] Respawning at {spawnPoint.name}. Current count: {currentEnemyCount}");
                // 3. ทำการ Spawn ศัตรูเต็มจำนวนใหม่ 
                SpawnEnemiesAtPoint(spawnPoint);
            }
        }
    }

    // ----------------------------------------------------
    // 📢 Server Logic: บันทึกศัตรูที่ถูกทำลาย
    // ----------------------------------------------------

    // Host/Server บันทึก ID ศัตรูที่ถูกทำลายลงใน NetworkList
    public void HandleEnemyDestroyed(Idestoryable destroyedObject)
    {
        if (!IsServer) return;

        if (destroyedObject is Character enemyCharacter)
        {
            // บันทึก ID ของศัตรูที่ตายแล้วลงใน NetworkList (ซิงค์ Late Joiner)
            ulong id = enemyCharacter.NetworkObjectId;
            if (!destroyedEnemyIds.Contains(id))
            {
                destroyedEnemyIds.Add(id);
                Debug.Log($"[SERVER] Enemy Destroyed: {id}. Tracking for late joiners.");
            }
        }
    }

    // ----------------------------------------------------
    // 💻 Client Logic: การตรวจสอบและทำลาย Object (สำหรับ Late Joiner)
    // ----------------------------------------------------

    private void HandleDestroyedEnemyListChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return; 
        CheckAndDestroyObject(changeEvent.Value);
    }
    
    private void CheckAndDestroyObject(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject netObj))
        {
             // ทำลาย GameObject ที่ถูก Spawn ขึ้นมาใหม่โดยไม่ตั้งใจ
             Destroy(netObj.gameObject);
             Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Destroyed Late Joiner Enemy ID: {networkId}");
        }
    }
}
//using System.Collections.Generic;
//using Unity.Netcode;
//using UnityEngine;

//public class ItemSpawner : NetworkBehaviour
//{
//    public static ItemSpawner Instance;
//    public List<ItemOBJ> itemPrefabs;
//    public Transform[] spawnPoints;
//    public int initialSpawnAmount = 5;

//    [Header("Spawn Settings")]
//    public float spawnRadius = 2f; // รัศมีการสุ่มรอบ spawn point
//    public bool randomRotation = true; // สุ่มการหมุน
//    public Vector2 spawnHeightRange = new Vector2(0f, 1f); // ความสูงในการ spawn

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Debug.LogWarning("Multiple ItemSpawner instances found!");
//            Destroy(gameObject);
//        }
//    }

//    public override void OnNetworkSpawn()
//    {
//        Debug.Log($"[ItemSpawner] OnNetworkSpawn - IsServer: {IsServer}");

//        if (IsServer)
//        {
//            SpawnInitialItems();
//        }
//    }

//    public void SpawnInitialItems()
//    {
//        if (itemPrefabs.Count == 0 || spawnPoints.Length == 0)
//        {
//            Debug.LogWarning("[ItemSpawner] No item prefabs or spawn points assigned!");
//            return;
//        }

//        Debug.Log($"[ItemSpawner] Spawning {initialSpawnAmount} items at {spawnPoints.Length} spawn points");

//        for (int i = 0; i < initialSpawnAmount; i++)
//        {
//            SpawnRandomItem();
//        }
//    }

//    private void SpawnRandomItem()
//    {
//        int itemIndex = Random.Range(0, itemPrefabs.Count);
//        int spawnPointIndex = Random.Range(0, spawnPoints.Length);

//        Transform spawnPoint = spawnPoints[spawnPointIndex];
//        Vector3 spawnPosition = GetRandomSpawnPosition(spawnPoint);
//        Quaternion spawnRotation = GetRandomRotation();

//        Debug.Log($"[ItemSpawner] Spawning item {itemPrefabs[itemIndex].item.itemName} at {spawnPosition}");

//        ItemOBJ obj = Instantiate(itemPrefabs[itemIndex], spawnPosition, spawnRotation);
//        obj.NetworkObject.Spawn(true);

//        // สุ่ม amount
//        int randomAmount = Random.Range(1, obj.item.maxStack + 1);
//        obj.SetAmount(randomAmount);

//        Debug.Log($"[ItemSpawner] Spawned {obj.item.itemName} x{randomAmount} at {spawnPoint.name}");
//    }

//    private Vector3 GetRandomSpawnPosition(Transform spawnPoint)
//    {
//        // สุ่มตำแหน่งภายในวงกลมรอบ spawn point
//        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
//        Vector3 randomPosition = spawnPoint.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

//        // เพิ่มความสูงแบบสุ่ม
//        randomPosition.y += Random.Range(spawnHeightRange.x, spawnHeightRange.y);

//        return randomPosition;
//    }

//    private Quaternion GetRandomRotation()
//    {
//        if (randomRotation)
//        {
//            // สุ่มการหมุนแกน Y
//            float randomYRotation = Random.Range(0f, 360f);
//            return Quaternion.Euler(0f, randomYRotation, 0f);
//        }
//        else
//        {
//            return Quaternion.identity;
//        }
//    }

//    // สำหรับ Spawn Item จาก Event อื่น เช่น การดรอป
//    public void SpawnItem(ItemSO item, int amount, Vector3 position)
//    {
//        if (!IsServer)
//        {
//            Debug.LogWarning("[ItemSpawner] Trying to spawn item from client!");
//            return;
//        }

//        // หา prefab ที่ตรงกับ item
//        ItemOBJ itemPrefab = itemPrefabs.Find(prefab => prefab.item == item);
//        if (itemPrefab == null)
//        {
//            Debug.LogWarning($"[ItemSpawner] Item prefab not found for {item.itemName}");
//            return;
//        }

//        Vector3 spawnPosition = position + new Vector3(
//            Random.Range(-spawnRadius, spawnRadius),
//            Random.Range(spawnHeightRange.x, spawnHeightRange.y),
//            Random.Range(-spawnRadius, spawnRadius)
//        );

//        Quaternion spawnRotation = GetRandomRotation();

//        Debug.Log($"[ItemSpawner] Spawning dropped item: {item.itemName} x{amount} at {spawnPosition}");

//        GameObject obj = Instantiate(itemPrefab.gameObject, spawnPosition, spawnRotation);
//        ItemOBJ itemObj = obj.GetComponent<ItemOBJ>();
//        itemObj.SetAmount(amount);
//        obj.GetComponent<NetworkObject>().Spawn(true);
//    }

//    // เมธอดสำหรับการ spawn item ที่ spawn point เฉพาะ
//    public void SpawnItemAtSpawnPoint(ItemSO item, int amount, int spawnPointIndex = -1)
//    {
//        if (!IsServer) return;

//        if (spawnPointIndex == -1)
//        {
//            spawnPointIndex = Random.Range(0, spawnPoints.Length);
//        }
//        else if (spawnPointIndex >= spawnPoints.Length)
//        {
//            Debug.LogWarning($"[ItemSpawner] Spawn point index {spawnPointIndex} out of range!");
//            spawnPointIndex = Random.Range(0, spawnPoints.Length);
//        }

//        Transform spawnPoint = spawnPoints[spawnPointIndex];
//        Vector3 spawnPosition = GetRandomSpawnPosition(spawnPoint);

//        SpawnItem(item, amount, spawnPosition);
//    }

//    // เมธอดสำหรับ respawn items
//    public void RespawnItems()
//    {
//        if (!IsServer) return;

//        Debug.Log("[ItemSpawner] Respawning items...");
//        SpawnInitialItems();
//    }

//    // สำหรับ debugging
//    private void OnDrawGizmosSelected()
//    {
//        if (spawnPoints != null)
//        {
//            Gizmos.color = Color.green;
//            foreach (Transform spawnPoint in spawnPoints)
//            {
//                if (spawnPoint != null)
//                {
//                    // วาดวงกลมแสดงพื้นที่ spawn
//                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);

//                    // วาดลูกศรแสดงทิศทาง
//                    Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 2f);

//                    // วาดจุด spawn
//                    Gizmos.color = Color.red;
//                    Gizmos.DrawSphere(spawnPoint.position, 0.2f);
//                    Gizmos.color = Color.green;
//                }
//            }
//        }
//    }

//    // เมธอดสำหรับการลบ items ทั้งหมด (สำหรับ testing)
//    // public void ClearAllItems()
//    // {
//    //     if (!IsServer) return;

//    //     ItemOBJ[] allItems = FindObjectsOfType<ItemOBJ>();
//    //     Debug.Log($"[ItemSpawner] Clearing {allItems.Length} items");

//    //     foreach (ItemOBJ item in allItems)
//    //     {
//    //         if (item.NetworkObject.IsSpawned)
//    //         {
//    //             item.NetworkObject.Despawn(true);
//    //         }
//    //     }
//    // }
//}
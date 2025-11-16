using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Linq;

public class EnemySpawnPoint : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public int maxEnemies = 3;
    public float spawnRadius = 10f;
    public float respawnInterval = 10f;
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    private bool hasSpawned = false;   // local เท่านั้น ไม่ต้อง sync
    private Coroutine respawnRoutine;

    private void Update()
    {
        if (!IsServer) return;

        if (!hasSpawned && CheckPlayersInRange())
        {
            SpawnEnemies();
            hasSpawned = true;
            StartRespawnLoop();
        }
    }

    /// <summary>
    /// เช็คว่ามี Player เข้ามาในระยะ spawn หรือไม่
    /// </summary>
    private bool CheckPlayersInRange()
    {
        var players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= spawnRadius)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Spawn ศัตรูในทุก spawnPoint (Host เท่านั้น)
    /// </summary>
    private void SpawnEnemies()
    {
        if (!IsServer) return;

        foreach (var spawnPoint in spawnPoints)
        {
            int currentEnemies = CountEnemiesAround(spawnPoint.position, 5f);
            int spawnAmount = Mathf.Max(0, maxEnemies - currentEnemies);

            for (int i = 0; i < spawnAmount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 2f;
                Vector3 spawnPos = spawnPoint.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

                var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }

    /// <summary>
    /// นับจำนวนศัตรูรอบ ๆ จุดเกิด
    /// </summary>
    private int CountEnemiesAround(Vector3 center, float radius)
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        return enemies.Count(e => Vector3.Distance(e.transform.position, center) <= radius);
    }

    /// <summary>
    /// เริ่มลูป respawn
    /// </summary>
    private void StartRespawnLoop()
    {
        if (respawnRoutine == null)
            respawnRoutine = StartCoroutine(RespawnLoop());
    }

    /// <summary>
    /// Respawn ศัตรูเรื่อย ๆ ถ้ายังมี player ใน scene
    /// </summary>
    private IEnumerator RespawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);

            if (!IsServer) yield break;

            if (CheckPlayersInRange())
                SpawnEnemies();
        }
    }
}

using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class SpawnTrigger : NetworkBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ชื่อ Tag ของ GameObject ที่จะทำให้เกิดการ Spawn (เช่น 'Player')")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("กำหนดว่าควรถูกทำลายตัวเองหลังจากเรียก Spawn แล้วหรือไม่")]
    [SerializeField] private bool destroyAfterTrigger = true;

    // **✅ เพิ่ม: Prefabs ศัตรูที่จะ Spawn**
    [Header("Spawn Content")]
    [Tooltip("อาร์เรย์ของ Prefab ศัตรูที่สามารถสุ่มสร้างได้")]
    [SerializeField] private GameObject[] enemyPrefabsToSpawn;

    // **✅ เพิ่ม: จุด Spawn เฉพาะสำหรับ Trigger นี้**
    [Tooltip("จุด Spawn สำหรับศัตรูชุดนี้ (ใช้ตำแหน่ง Transform)")]
    [SerializeField] private Transform[] specificSpawnPoints;

    // 💡 สถานะ: ป้องกันการเรียก Spawn ซ้ำหลายครั้ง
    private bool hasTriggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogError($"Collider บน {gameObject.name} ไม่ได้ถูกตั้งค่าเป็น 'Is Trigger'!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(targetTag))
        {
            if (other.TryGetComponent(out NetworkObject netObj) && netObj.IsOwner)
            {
                // **✅ ผู้เล่น Local Client ตรวจพบ Trigger**

                // 💡 ตรวจสอบความถูกต้องของข้อมูลก่อนส่ง RPC
                if (specificSpawnPoints == null || specificSpawnPoints.Length == 0 ||
                    enemyPrefabsToSpawn == null || enemyPrefabsToSpawn.Length == 0)
                {
                    Debug.LogError("Spawn Trigger ไม่มีจุด Spawn หรือ Prefab กำหนดไว้!");
                    return;
                }

                // 💡 ส่งคำสั่งไปยัง Server เพื่อเรียก Spawn
                // เราไม่สามารถส่ง Transform[] และ GameObject[] ผ่าน RPC ได้โดยตรง
                // เราจะส่ง ID ของ NetworkObject ของ Trigger ตัวเองไปให้ Server ดึงข้อมูลแทน
                RequestSpawnOnServerRpc(NetworkObject.NetworkObjectId);

                hasTriggered = true;

                if (destroyAfterTrigger)
                {
                    // ทำลาย GameObject นี้หลังจากการเรียกใช้งาน (บน Client)
                    // การทำลายบน Server จะเกิดขึ้นเมื่อ Server ได้รับ RPC
                    // เนื่องจากเป็นวัตถุที่ไม่ใช่ศัตรูหรือผู้เล่น, Destroy() ธรรมดาก็เพียงพอ
                    Destroy(gameObject);
                }
            }
        }
    }

    // ----------------------------------------------------
    // 📢 RPC Call: ส่งคำสั่งจาก Client ไปยัง Server เพื่อเรียก Spawn
    // ----------------------------------------------------

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSpawnOnServerRpc(ulong triggerNetworkId)
    {
        // 4. Server ดึงข้อมูล SpawnTrigger จาก NetworkID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(triggerNetworkId, out NetworkObject netObj) &&
            netObj.TryGetComponent(out SpawnTrigger trigger))
        {
            // 5. Server ทำการเรียก Spawn ผ่าน EnemySpawnManager Singleton
            if (EnemySpawnManager.Instance != null)
            {
                // **✅ เรียกเมธอดใหม่ใน Manager พร้อมส่งข้อมูล**
                EnemySpawnManager.Instance.OnTriggerSpawn(
                    trigger.specificSpawnPoints,
                    trigger.enemyPrefabsToSpawn
                );
                Debug.Log($"[SERVER] Trigger Spawn activated by RPC on trigger ID: {triggerNetworkId}.");
            }
            else
            {
                Debug.LogError("EnemySpawnManager Instance is null on the server.");
            }
        }
        else
        {
            Debug.LogError($"[SERVER] Failed to find SpawnTrigger with ID: {triggerNetworkId}");
        }
    }
}
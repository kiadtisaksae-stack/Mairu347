using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TeleportPoint — Component ที่ถูกเพิ่มอัตโนมัติโดย TeleportSetup
/// ไม่ต้องเพิ่มเอง — ระบบจัดการให้ทั้งหมด
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TeleportPoint : MonoBehaviour
{
    [Header("Auto-configured by TeleportSetup")]
    [Tooltip("จุดปลายทางที่จะ teleport ไป")]
    public Transform destination;

    [Header("Settings")]
    [Tooltip("Cooldown ป้องกัน teleport วนลูป (วินาที)")]
    public float cooldown = 3f;

    // ─── Anti-Loop System ───
    // ใช้ static dictionary เพื่อให้ทุก TeleportPoint รู้ว่า player คนไหน
    // เพิ่ง teleport มา — ป้องกันการ warp วนลูป A↔B
    private static readonly Dictionary<int, float> playerLastTeleportTime = new Dictionary<int, float>();

    /// <summary>
    /// เรียกเมื่อ player เพิ่ง teleport ไปถึง — mark ว่าห้าม teleport ซ้ำ
    /// </summary>
    public static void MarkPlayerArrived(GameObject player)
    {
        int id = player.GetInstanceID();
        playerLastTeleportTime[id] = Time.time;
    }

    /// <summary>
    /// เช็คว่า player ยังอยู่ใน cooldown หรือไม่
    /// </summary>
    private bool IsPlayerInCooldown(GameObject player)
    {
        int id = player.GetInstanceID();
        if (!playerLastTeleportTime.ContainsKey(id)) return false;
        return Time.time < playerLastTeleportTime[id] + cooldown;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destination == null) return;
        if (!other.CompareTag("Player")) return;

        // ─── Anti-Loop: ถ้า player เพิ่ง teleport มา → ข้าม ───
        if (IsPlayerInCooldown(other.gameObject)) return;

        Player player = other.GetComponent<Player>();
        if (player == null || !player.IsOwner) return;

        // Mark ว่า player กำลังจะ teleport (ป้องกัน loop ที่ปลายทาง)
        MarkPlayerArrived(other.gameObject);

        // ส่งคำสั่ง teleport ผ่าน TeleportManager
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.RequestTeleport(
                player,
                destination.position,
                destination.rotation
            );
            Debug.Log($"🌀 TeleportPoint: {player.Name} → {destination.name}");
        }
        else
        {
            Debug.LogWarning("TeleportManager.Instance is null!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (destination != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, destination.position);
            Gizmos.DrawWireSphere(destination.position, 0.3f);
        }
    }
}

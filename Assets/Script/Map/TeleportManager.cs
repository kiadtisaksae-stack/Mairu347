using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// TeleportManager — Singleton ติดอยู่กับ NetworkManager, ข้าม scene ได้
/// ทำหน้าที่เดียว: รับคำสั่ง teleport แล้วย้ายผู้เล่นผ่าน RPC
/// </summary>
public class TeleportManager : NetworkBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Debug")]
    public bool debugMode = false;

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

    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
        base.OnNetworkDespawn();
    }

    // ─────────────────────────────────────────────
    // Public API — เรียกจาก TeleportPoint
    // ─────────────────────────────────────────────

    /// <summary>
    /// Client เรียกเมื่อเหยียบจุด warp → ส่ง RPC ไป Server
    /// </summary>
    public void RequestTeleport(Player player, Vector3 destination, Quaternion rotation)
    {
        if (player == null || !player.IsOwner) return;
        RequestTeleportServerRpc(player.NetworkObjectId, destination, rotation);
    }

    // ─────────────────────────────────────────────
    // RPC
    // ─────────────────────────────────────────────

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestTeleportServerRpc(ulong playerId, Vector3 destination, Quaternion rotation)
    {
        if (!IsServer) return;
        Log($"🚀 Server received teleport request for player {playerId}");
        ExecuteTeleportClientRpc(playerId, destination, rotation);
    }

    [ClientRpc]
    private void ExecuteTeleportClientRpc(ulong playerId, Vector3 destination, Quaternion rotation)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerObj))
            return;

        Player player = playerObj.GetComponent<Player>();
        if (player == null || !player.IsOwner) return;

        // ปิด CharacterController ก่อนย้าย
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(destination, rotation);

        if (cc != null) cc.enabled = true;

        Log($"✅ Teleported {player.Name} to {destination}");
    }

    // ─────────────────────────────────────────────
    // Utility
    // ─────────────────────────────────────────────

    public void UnregisterPlayer(Player player)
    {
        // สำหรับ backward compatibility กับ Player.OnNetworkDespawn
    }

    private void Log(string msg)
    {
        if (debugMode) Debug.Log($"[TeleportManager] {msg}");
    }
}
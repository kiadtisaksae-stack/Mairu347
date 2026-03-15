using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Teleport : NetworkBehaviour
{
    [Header("📍 Teleport Settings")]
    public Transform destinationPoint;
    public float teleportDelay = 3f;

    [Header("🎯 Map Boundary Check")]
    public BoxCollider map0Boundary;

    [Header("✨ Visual Effects")]
    public ParticleSystem teleportEffect;
    public Light teleportLight;
    public AudioClip teleportSound;

    [Header("🔒 Anti-Spam")]
    public float cooldownTime = 5f;

    private NetworkVariable<bool> isMap0Active = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isReady = true;
    private Coroutine teleportCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!isReady) return;
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.IsOwner)
            {
                RequestTeleportServerRpc(player.NetworkObjectId);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
            ResetTeleport();
            Debug.Log("❌ Teleport cancelled - player left area");
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestTeleportServerRpc(ulong playerId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerNetObj))
        {
            Player player = playerNetObj.GetComponent<Player>();
            if (player != null)
            {
                // แก้ปัญหา #18 — เดิมเรียก TeleportPlayerServerRpc (ServerRpc) จาก Server
                // ServerRpc เรียก ServerRpc ซ้อนกันไม่ถูกต้อง
                // แก้โดยเรียก ClientRpc โดยตรงจาก Server แทน
                TeleportPlayerClientRpc(player.NetworkObjectId, destinationPoint.position, destinationPoint.rotation);
            }
        }
    }

    // ลบ TeleportPlayerServerRpc ออก — ไม่จำเป็นแล้ว
    // Server เรียก ClientRpc โดยตรงได้เลย

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong playerId, Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerObj))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null && player.IsOwner)
            {
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                    player.transform.SetPositionAndRotation(position, rotation);
                    controller.enabled = true;
                }
                else
                {
                    player.transform.SetPositionAndRotation(position, rotation);
                }
                Debug.Log($"✅ Teleported {player.Name} to {position}");
            }
        }
    }

    private void StopTeleportEffects()
    {
        if (teleportEffect != null) teleportEffect.Stop();
        if (teleportLight != null) teleportLight.enabled = false;
    }

    private void ResetTeleport()
    {
        isReady = true;
        StopTeleportEffects();
    }

    private void OnDrawGizmos()
    {
        if (destinationPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destinationPoint.position);
            DrawArrow(transform.position, destinationPoint.position - transform.position);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(destinationPoint.position, Vector3.one * 1f);
        }
        if (map0Boundary != null)
        {
            Gizmos.color = isMap0Active.Value ? Color.green : Color.red;
            Gizmos.DrawWireCube(map0Boundary.transform.position + map0Boundary.center, map0Boundary.size);
        }
        Gizmos.color = Color.yellow;
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
    }

    private void DrawArrow(Vector3 pos, Vector3 direction)
    {
        float arrowHeadLength = 0.5f;
        float arrowHeadAngle = 20.0f;
        Gizmos.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
}
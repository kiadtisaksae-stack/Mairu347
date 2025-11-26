using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;

public class Teleport : NetworkBehaviour
{
    [Header("📍 Teleport Settings")]
    public Transform destinationPoint;    // จุดปลายทาง
    public float teleportDelay = 3f;      

    [Header("🗺️ Map Management")]
    public GameObject mapToActivate;      // Map ที่จะเปิด (ปลายทาง)
    public GameObject mapToDeactivate;    // Map ที่จะปิด (ต้นทาง) - Optional

    [Header("🎯 Map Boundary Check")]
    public BoxCollider map0Boundary;      // ลาก BoxCollider ของ Map0 มาวางที่นี่

    [Header("✨ Visual Effects")]
    public ParticleSystem teleportEffect;
    public Light teleportLight;
    public AudioClip teleportSound;

    [Header("🔒 Anti-Spam")]
    public float cooldownTime = 5f;       // ป้องกันวาปไปมาถี่เกิน

    private NetworkVariable<bool> isMap0Active = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isReady = true;
    private Coroutine teleportCoroutine;

    public override void OnNetworkSpawn()
    {
        isMap0Active.OnValueChanged += OnMap0ActiveChanged;
        UpdateMap0Visibility();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. ตรวจสอบ Anti-Spam และ Player Tag
        if (!isReady) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            // 2. ตรวจสอบว่าเป็น Local Player (Owner) ที่เหยียบหรือไม่
            if (player != null && player.IsOwner)
            {
                // 3. 🚨 NEW: Client ส่งคำขอวาร์ปไปยัง Server ทันที
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

    private IEnumerator TeleportSequence(Player player)
    {
        Debug.Log($"🚀 Starting teleport sequence to {destinationPoint.name}");

        isReady = false;

        StartTeleportEffectsClientRpc();
        yield return new WaitForSeconds(teleportDelay);

        int playerCountInMap0 = CountPlayersInMap0();
        Debug.Log($"👥 Players in Map0: {playerCountInMap0}");

        ExecuteTeleport(player);

        if (playerCountInMap0 > 1)
        {
            Debug.Log("🔵 Multiple players in Map0 - Keeping it active");
        }
        else
        {
            SetMap0ActiveServerRpc(false);
            Debug.Log("🔴 Only one player in Map0 - Deactivating it");
        }

        yield return new WaitForSeconds(cooldownTime);
        isReady = true;

        Debug.Log("✅ Teleport pad ready again");
    }

    private int CountPlayersInMap0()
    {
        if (map0Boundary == null)
        {
            Debug.LogError("❌ Map0 Boundary is not assigned!");
            return 0;
        }

        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Player player in allPlayers)
        {
            if (map0Boundary.bounds.Contains(player.transform.position))
            {
                count++;
            }
        }

        return count;
    }

    [ServerRpc]
    private void SetMap0ActiveServerRpc(bool active)
    {
        isMap0Active.Value = active;
    }

    private void OnMap0ActiveChanged(bool oldValue, bool newValue)
    {
        UpdateMap0Visibility();
        Debug.Log($"🗺️ Map0 is now {(newValue ? "ACTIVE" : "INACTIVE")}");
    }

    private void UpdateMap0Visibility()
    {
        if (mapToDeactivate != null)
        {
            mapToDeactivate.SetActive(isMap0Active.Value);
        }
    }

    [ClientRpc]
    private void StartTeleportEffectsClientRpc()
    {
        if (teleportEffect != null)
            teleportEffect.Play();

        if (teleportLight != null)
            teleportLight.enabled = true;

        if (teleportSound != null)
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
    }

    private void ExecuteTeleport(Player player)
    {
        if (destinationPoint == null)
        {
            Debug.LogError("❌ Destination point is not assigned!");
            return;
        }

        // ❌ ลบ Logic การเปิด Map ที่นี่ออก
        // if (mapToActivate != null && !mapToActivate.activeSelf) { mapToActivate.SetActive(true); ... }

        // 💡 แค่วาร์ป Player ไป
        TeleportPlayerServerRpc(player.NetworkObjectId);
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
                // 💡 1. Host/Server ตรวจสอบว่า Map ปลายทางเปิดอยู่หรือไม่
                if (mapToActivate != null && !mapToActivate.activeSelf)
                {
                    // 🚨 ถ้าปิดอยู่: สั่ง Host/Server เปิด Map ก่อนวาร์ป
                    OpenDestinationMapServerRpc();

                    // 2. Host/Server รอ 1 เฟรม (เผื่อการซิงค์ GameObject) 
                    // แล้วเริ่ม Coroutine วาร์ป
                    StartCoroutine(ExecuteTeleportSequenceDelayed(player));
                }
                else
                {
                    // 3. Map เปิดอยู่แล้ว หรือไม่มี Map ให้เปิด: เริ่มวาร์ปทันที
                    teleportCoroutine = StartCoroutine(TeleportSequence(player));
                }
            }
        }
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void OpenDestinationMapServerRpc()
    {
        // โค้ดนี้รันบน Host/Server เท่านั้น
        if (mapToActivate != null && !mapToActivate.activeSelf)
        {
            // 1. Host เปิด Map
            mapToActivate.SetActive(true);

            // 2. สั่ง Client RPC เพื่อให้ทุกคนเปิด Map นี้ด้วย
            SyncMapActivationClientRpc(mapToActivate.name, true);
        }
    }

    // 💡 NEW: Client RPC เพื่อให้ Client เปิด Map ตาม Host
    [ClientRpc]
    private void SyncMapActivationClientRpc(string mapName, bool active)
    {
        // ตรวจสอบว่า Map ที่ถูกสั่งเปิด/ปิดตรงกับ mapToActivate หรือ mapToDeactivate หรือไม่
        if (mapToActivate != null && mapToActivate.name == mapName)
        {
            mapToActivate.SetActive(active);
        }
        // (เพิ่ม logic สำหรับ mapToDeactivate ถ้าจำเป็นต้องปิด Map ต้นทางทันที)
    }

    // 💡 NEW: Coroutine ที่ใช้เรียกหลังเปิด Map เพื่อหน่วงเวลา
    private IEnumerator ExecuteTeleportSequenceDelayed(Player player)
    {
        // รอ 1 เฟรม ให้ Netcode มีโอกาสซิงค์สถานะ Map Activation
        yield return null;

        // เริ่ม Coroutine วาร์ปหลัก
        teleportCoroutine = StartCoroutine(TeleportSequence(player));
    }

    [ServerRpc]
    private void TeleportPlayerServerRpc(ulong playerId)
    {
        TeleportPlayerClientRpc(playerId, destinationPoint.position, destinationPoint.rotation);
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong playerId, Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject playerObj))
        {
            Player player = playerObj.GetComponent<Player>();
            if (player != null && player.IsOwner)
            {
                // ใช้ CharacterController สำหรับการย้ายที่ถูกต้อง
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
        if (teleportEffect != null)
            teleportEffect.Stop();

        if (teleportLight != null)
            teleportLight.enabled = false;
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
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
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
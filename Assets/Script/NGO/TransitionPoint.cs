using UnityEngine;
using Unity.Netcode;

// แนบกับ Collider ที่ตั้งค่า isTrigger = true
public class TransitionPoint : NetworkBehaviour
{
    [SerializeField] private string targetSceneName = "NewGameScene";

    // ตรวจสอบ Player ที่ชน
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            // 1. ตรวจสอบว่าเป็น Local Player ที่กำลังควบคุมอยู่หรือไม่
            if (player.IsOwner)
            {
                // 2. Local Player ส่งคำขอไปหา Server ทันที
                RequestSceneChangeServerRpc(targetSceneName);
            }
        }
    }

    // ServerRpc: ถูกส่งจาก Client ไปรันบน Host/Server
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestSceneChangeServerRpc(string sceneName)
    {
        // 3. Server ตรวจสอบสิทธิ์ (เช่น ตรวจสอบว่ามีผู้เล่นอื่นกำลังอยู่ในระหว่างโหลดหรือไม่)
        if (!IsServer) return;

        Debug.Log($"[SERVER] Client {OwnerClientId} requested scene load: {sceneName}");

        // 4. Server สั่งให้ NetworkSceneManager โหลด Scene ใหม่สำหรับทุกคน
        SceneTransitionHandler.Instance.LoadGameScene(sceneName);
    }
}
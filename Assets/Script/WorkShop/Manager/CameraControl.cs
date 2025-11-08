using UnityEngine;
using Unity.Netcode;

public class CameraControl : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10);

    private Transform target;

    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        TryFindLocalPlayer();
    }

    void Update()
    {
        TryFindLocalPlayer();
    }

    private void TryFindLocalPlayer()
    {
        // ✅ ถ้าเจอแล้ว ไม่ต้องหาอีก
        if (target != null) return;

        // ✅ หาเฉพาะ Player ที่เป็น Owner ของเครื่องนี้
        foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            var netObj = player.GetComponent<NetworkObject>();

            if (netObj != null && netObj.IsOwner)
            {
                target = player.transform;
                Debug.Log("[Camera] Found local player, following.");
                return;
            }
        }

        // ยังไม่เจอ แสดงสถานะรอ
        // Debug.Log("[Camera] Waiting for local player spawn...");
    }

    void FixedUpdate()
    {
        Follow();
    }

    private void Follow()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

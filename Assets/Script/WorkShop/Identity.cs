using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
public class Identity : NetworkBehaviour
{
    private readonly NetworkVariable<FixedString32Bytes> _networkName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    protected NetworkVariable<bool> isOnLive = new NetworkVariable<bool>(true);

    public string Name
    {
        get => _networkName.Value.ToString();
        set
        {
            if (IsServer)
            {
                _networkName.Value = new FixedString32Bytes(value);
                gameObject.name = value;
            }
        }
    }

    public int positionX
    {
        get { return Mathf.RoundToInt(transform.position.x); }
        set
        {
            if (IsOwner)
                transform.position = new Vector3(value, transform.position.y, transform.position.z);
        }
    }

    public int positionY
    {
        get { return Mathf.RoundToInt(transform.position.z); }
        set
        {
            if (IsOwner)
                transform.position = new Vector3(transform.position.x, transform.position.y, value);
        }
    }

    public string getIdentityInfo()
    {
        return $"Name: {Name}, Position: ({transform.position.x}, {transform.position.y}, {transform.position.z})";
    }

    #region --- Local Player and Closest Player ---
    private Transform _localPlayerTransform;
    protected Transform LocalPlayerTransform
    {
        get
        {
            if (_localPlayerTransform == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
            {
                var localPlayerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (localPlayerNetworkObject != null)
                    _localPlayerTransform = localPlayerNetworkObject.transform;
            }
            return _localPlayerTransform;
        }
    }

    protected Player LocalPlayer
    {
        get
        {
            if (LocalPlayerTransform != null)
                return LocalPlayerTransform.GetComponent<Player>();
            return null;
        }
    }

    protected float GetDistanPlayer()
    {
        if (LocalPlayerTransform == null) return -1;
        return Vector3.Distance(transform.position, LocalPlayerTransform.position);
    }

    protected Transform ClosestPlayerTransform => GetClosestPlayerTransform();

    protected Player GetClosestPlayer()
    {
        Transform closestTransform = ClosestPlayerTransform;
        if (closestTransform != null)
            return closestTransform.GetComponent<Player>();
        return null;
    }

    protected float GetDistanClosestPlayer()
    {
        Transform closestTransform = ClosestPlayerTransform;
        if (closestTransform == null) return float.MaxValue;
        return Vector3.Distance(transform.position, closestTransform.position);
    }

    protected Transform GetClosestPlayerTransform()
    {
        if (NetworkManager.Singleton == null) return null;

        float minDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                Transform playerTransform = client.PlayerObject.transform;
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPlayer = playerTransform;
                }
            }
        }
        return closestPlayer;
    }
    #endregion

    #region --- Identity InFront Caching ---
    protected Identity _cachedIdentityInFront;
    public Identity InFront => _cachedIdentityInFront;

    float sphereRadius = 0.5f;
    float maxDistance = 1.0f;

    private NetworkVariable<Vector3> savedPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float saveInterval = 5f;
    private float lastSaveTime = 0f;
    private Vector3 _lastSavedPosition = Vector3.zero;

    [ServerRpc]
    private void SendPositionToServerRpc(Vector3 pos)
    {
        // แก้ปัญหา #14 — ลบ if (!IsOwner) ออก เพราะ ServerRpc ต้องการ IsOwner อยู่แล้ว
        // เพิ่มเช็ค IsSpawned ก็พอ
        if (!IsSpawned) return;
        savedPosition.Value = pos;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && string.IsNullOrEmpty(_networkName.Value.ToString()))
        {
            _networkName.Value = gameObject.name;
        }

        if (IsOwner)
        {
            StartCoroutine(RestorePositionAfterReconnect());
        }

        SetUP();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (gameObject != null)
            Destroy(gameObject);
    }

    private IEnumerator RestorePositionAfterReconnect()
    {
        yield return new WaitForSeconds(0.2f);

        if (savedPosition.Value != Vector3.zero)
        {
            Vector3 pos = savedPosition.Value;
            if (Physics.Raycast(pos + Vector3.up * 2, Vector3.down, out RaycastHit hit, 10))
                transform.position = hit.point;
            else
                transform.position = pos;

            Debug.Log($"✅ Restored player position to {transform.position}");
        }
    }

    // แก้ปัญหา #19 — ลบ Update ออกจาก Identity base class
    // Player.cs override และเรียก UpdateInFrontCache() ใน Update ของตัวเองอยู่แล้ว
    // ถ้า Identity ก็มี Update → SphereCast รันซ้ำสองรอบทุก frame
    // คลาสที่ไม่ใช่ Player (เช่น Enemy) ไม่ต้องการ InFront cache
    // void Update() { UpdateInFrontCache(); }  ← ลบออก

    private void FixedUpdate()
    {
        // แก้ปัญหา #14 — เช็คว่า position เปลี่ยนจริงก่อนส่ง RPC
        // ลดการส่ง RPC โดยไม่จำเป็น
        if (!IsOwner) return;

        if (Time.time >= lastSaveTime + saveInterval)
        {
            lastSaveTime = Time.time;

            // ส่งเฉพาะตอนที่ position เปลี่ยนจริงๆ
            if (Vector3.Distance(transform.position, _lastSavedPosition) > 0.1f)
            {
                _lastSavedPosition = transform.position;
                SendPositionToServerRpc(transform.position);
            }
        }
    }

    public virtual void SetUP()
    {
        if (IsServer)
            isOnLive.Value = true;
        SetIsOnLive(true);
    }

    // ทำให้เป็น protected เพื่อให้ Player.cs เรียกได้
    protected void UpdateInFrontCache()
    {
        RaycastHit hit = GetClosestInfornt();
        if (hit.collider != null)
            _cachedIdentityInFront = hit.collider.GetComponent<Identity>();
        else
            _cachedIdentityInFront = null;
    }

    public virtual RaycastHit GetClosestInfornt()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sphereRadius, transform.forward, maxDistance);
        RaycastHit closestHit = new RaycastHit();
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject != gameObject && hit.collider.GetComponent<Identity>() != null)
            {
                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    closestHit = hit;
                }
            }
        }
        return closestHit;
    }

    private void OnDrawGizmos()
    {
        Vector3 endPosition = transform.position + transform.forward * maxDistance;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(endPosition, sphereRadius);

        if (_cachedIdentityInFront != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_cachedIdentityInFront.transform.position, sphereRadius * 1.5f);
        }
    }
    #endregion

    protected void SetIsOnLive(bool value)
    {
        if (IsServer)
            isOnLive.Value = value;
        else
            SetIsOnLiveServerRpc(value);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsOnLiveServerRpc(bool value)
    {
        isOnLive.Value = value;
    }
}
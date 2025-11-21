using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using System.Collections;

// คลาสพื้นฐานสำหรับเอนทิตี้ที่รองรับ Netcode
public class Identity : NetworkBehaviour
{
    // 1. ใช้ NetworkVariable สำหรับข้อมูลที่ต้องซิงค์
    private readonly NetworkVariable<FixedString32Bytes> _networkName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server); // ⬅️ เปลี่ยนเป็น Server Write
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
            {
                transform.position = new Vector3(value, transform.position.y, transform.position.z);
            }
        }
    }
    public int positionY
    {
        get { return Mathf.RoundToInt(transform.position.z); }
        set
        {
            if (IsOwner)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, value);
            }
        }
    }

    public string getIdentityInfo()
    {
        return $"Name: {Name}, Position: ({transform.position.x}, {transform.position.y}, {transform.position.z})";
    }

    #region ---Local Player and Closest Player ---
    private Transform _localPlayerTransform;
    protected Transform LocalPlayerTransform
    {
        get
        {
            if (_localPlayerTransform == null && NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null)
            {
                var localPlayerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (localPlayerNetworkObject != null)
                {
                    _localPlayerTransform = localPlayerNetworkObject.transform;
                }
            }
            return _localPlayerTransform;
        }
    }

    private float distanFormPlayer;
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
        distanFormPlayer = Vector3.Distance(transform.position, LocalPlayerTransform.position);
        return distanFormPlayer;
    }

    protected Transform ClosestPlayerTransform => GetClosestPlayerTransform();

    protected Player GetClosestPlayer()
    {
        Transform closestTransform = ClosestPlayerTransform;
        if (closestTransform != null)
        {
            return closestTransform.GetComponent<Player>();
        }
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

    #region  --- Identity InFront Caching ---
    protected Identity _cachedIdentityInFront;
    public Identity InFront => _cachedIdentityInFront;

    float sphereRadius = 0.5f;
    float maxDistance = 1.0f;

    private float updateCheckInterval = 0.2f;
    private float lastCheckTime = 0f;

    private NetworkVariable<Vector3> savedPosition = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private float saveInterval = 5f;
    private float lastSaveTime = 0f;

    [ServerRpc]
    private void SendPositionToServerRpc(Vector3 pos)
    {
        if (!IsOwner || !IsSpawned) return;
        savedPosition.Value = pos;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && string.IsNullOrEmpty(_networkName.Value.ToString()))
        {
            _networkName.Value = gameObject.name;
        }

        // ✅ ถ้ามีตำแหน่ง restore ให้เรียก coroutine
        if (IsOwner)
        {
            StartCoroutine(RestorePositionAfterReconnect());
        }

        SetUP();
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();


        // 💡 รับประกันการทำลาย GameObject หลังจากการ Despawn ของ Netcode
        if (gameObject != null)
        {
            Destroy(gameObject);
        }

    }

    private IEnumerator RestorePositionAfterReconnect()
    {
        yield return new WaitForSeconds(0.2f);

        if (savedPosition.Value != Vector3.zero)
        {
            Vector3 pos = savedPosition.Value;

            if (Physics.Raycast(pos + Vector3.up * 2, Vector3.down, out RaycastHit hit, 10))
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = pos;
            }

            Debug.Log($"✅ Restored player position to {transform.position}");
        }
    }

    void Update()
    {
        UpdateInFrontCache();
    }

    private void FixedUpdate()
    {
        
        if (Time.time > lastCheckTime + updateCheckInterval)
        {
            lastCheckTime = Time.time;
        }
        

        if (Time.time >= lastSaveTime + saveInterval)
        {
            if (!IsOwner) return;
            lastSaveTime = Time.time;
            SendPositionToServerRpc(transform.position);
        }
    }

    public virtual void SetUP() 
    {
        if (IsServer)
        {
            isOnLive.Value = true;
        }
        SetIsOnLive(true);
    }

    protected void UpdateInFrontCache()
    {
        RaycastHit hit = GetClosestInfornt();
        if (hit.collider != null)
        {
            _cachedIdentityInFront = hit.collider.GetComponent<Identity>();
        }
        else
        {
            _cachedIdentityInFront = null;
        }
        
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
        {
            isOnLive.Value = value;
        }
        else
        {
            // ถ้าเป็น Client ส่ง RPC ไปหา Server
            SetIsOnLiveServerRpc(value);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetIsOnLiveServerRpc(bool value)
    {
        isOnLive.Value = value;
    }
}

using UnityEngine;
using Unity.Netcode;

public class CameraControl : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10);

    private Transform target;

    void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
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
        if (target == null)
        {
            TryFindLocalPlayer();
        }
    }
    private void TryFindLocalPlayer()
    {
        if (target != null) return;

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

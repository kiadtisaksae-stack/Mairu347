using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class TeleportManager : NetworkBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Map Boundaries")]
    public List<GameObject> mapBoundaries = new List<GameObject>();

    [Header("Debug")]
    public bool debugMode = true;

    private NetworkVariable<bool> isTeleporting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Dictionary<GameObject, List<Player>> playersInMap = new Dictionary<GameObject, List<Player>>();

    private void Awake()
    {
        // ไม่ตั้ง Instance ใน Awake อีกต่อไป — ย้ายไป OnNetworkSpawn แล้ว
        DontDestroyOnLoad(gameObject);
    }

    // แก้ปัญหา Static + NetworkBehaviour
    // ตั้ง Instance ตอน Spawn เท่านั้น
    public override void OnNetworkSpawn()
    {
        // ✅ ตั้ง Instance ตอน Spawn
        Instance = this;

        if (IsServer)
        {
            InitializeMapTracking();
        }
    }

    public override void OnNetworkDespawn()
    {
        // ✅ ล้าง Instance ตอน Despawn
        if (Instance == this)
            Instance = null;

        base.OnNetworkDespawn();
    }

    private void InitializeMapTracking()
    {
        foreach (GameObject mapBoundary in mapBoundaries)
        {
            if (mapBoundary != null && !playersInMap.ContainsKey(mapBoundary))
            {
                playersInMap[mapBoundary] = new List<Player>();
            }
        }
        Log("🗺️ Initialized map tracking");
    }

    public void RequestTeleport(Player requestingPlayer, GameObject currentMap, GameObject targetMap)
    {
        if (!IsServer) return;

        if (isTeleporting.Value)
        {
            Log("🚫 Teleport request denied - Another teleport in progress");
            return;
        }

        StartCoroutine(TeleportProcess(requestingPlayer, currentMap, targetMap));
    }

    private System.Collections.IEnumerator TeleportProcess(Player player, GameObject currentMap, GameObject targetMap)
    {
        isTeleporting.Value = true;
        Log($"🚀 Starting teleport process: {player.Name}");

        int playerCountInCurrentMap = CountPlayersInMap(currentMap);
        Log($"👥 Players in current map: {playerCountInCurrentMap}");

        yield return StartCoroutine(MovePlayerToMap(player, currentMap, targetMap));

        if (playerCountInCurrentMap <= 1)
        {
            yield return StartCoroutine(DeactivateMapSafely(currentMap));
        }
        else
        {
            Log($"🔵 Keeping map active - {playerCountInCurrentMap} players remaining");
        }

        ActivateMap(targetMap);

        isTeleporting.Value = false;
        Log("✅ Teleport process completed");
    }

    private System.Collections.IEnumerator MovePlayerToMap(Player player, GameObject currentMap, GameObject targetMap)
    {
        RemovePlayerFromMap(player, currentMap);
        AddPlayerToMap(player, targetMap);
        yield return null;
        Log($"📍 Moved {player.Name} from {currentMap.name} to {targetMap.name}");
    }

    private System.Collections.IEnumerator DeactivateMapSafely(GameObject map)
    {
        if (map == null) yield break;
        int finalPlayerCount = CountPlayersInMap(map);
        if (finalPlayerCount > 0)
        {
            Log($"🚫 Cancelled deactivation - {finalPlayerCount} players still in {map.name}");
            yield break;
        }
        map.SetActive(false);
        Log($"🔴 Deactivated map: {map.name}");
        yield return null;
    }

    private void ActivateMap(GameObject map)
    {
        if (map != null && !map.activeSelf)
        {
            map.SetActive(true);
            Log($"🟢 Activated map: {map.name}");
        }
    }

    public void AddPlayerToMap(Player player, GameObject map)
    {
        if (player == null || map == null) return;
        if (!playersInMap.ContainsKey(map))
            playersInMap[map] = new List<Player>();
        if (!playersInMap[map].Contains(player))
        {
            playersInMap[map].Add(player);
            Log($"➕ Added {player.Name} to {map.name}");
        }
    }

    public void RemovePlayerFromMap(Player player, GameObject map)
    {
        if (player == null || map == null || !playersInMap.ContainsKey(map)) return;
        if (playersInMap[map].Contains(player))
        {
            playersInMap[map].Remove(player);
            Log($"➖ Removed {player.Name} from {map.name}");
        }
    }

    public int CountPlayersInMap(GameObject map)
    {
        if (map == null || !playersInMap.ContainsKey(map)) return 0;
        return playersInMap[map].Count;
    }

    public void RegisterPlayerToMap(Player player, GameObject initialMap)
    {
        if (IsServer && player != null && initialMap != null)
            AddPlayerToMap(player, initialMap);
    }

    public void UnregisterPlayer(Player player)
    {
        if (IsServer && player != null)
        {
            foreach (var map in playersInMap.Keys)
                RemovePlayerFromMap(player, map);
            Log($"👋 Unregistered player: {player.Name}");
        }
    }

    private void Log(string message)
    {
        if (debugMode) Debug.Log($"[TeleportManager] {message}");
    }

    private void OnGUI()
    {
        if (debugMode && IsServer)
        {
            GUILayout.BeginArea(new Rect(10, 100, 300, 400));
            GUILayout.Label("🗺️ TELEPORT MANAGER DEBUG");
            GUILayout.Label($"IsTeleporting: {isTeleporting.Value}");
            foreach (var map in playersInMap.Keys)
                GUILayout.Label($"{map.name}: {playersInMap[map].Count} players");
            GUILayout.EndArea();
        }
    }
}
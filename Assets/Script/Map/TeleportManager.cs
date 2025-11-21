using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class TeleportManager : NetworkBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Map Boundaries")]
    public List<GameObject> mapBoundaries = new List<GameObject>(); // กรอบแต่ละ map

    [Header("Debug")]
    public bool debugMode = true;

    // NetworkVariable สำหรับเก็บสถานะการใช้งาน Teleport
    private NetworkVariable<bool> isTeleporting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Dictionary<GameObject, List<Player>> playersInMap = new Dictionary<GameObject, List<Player>>();

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
        // เริ่มต้น tracking ผู้เล่นในแต่ละ map
        if (IsServer)
        {
            InitializeMapTracking();
        }
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

    // ✅ เรียกจาก Teleport Pad เมื่อต้องการเทเลพอร์ต
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

        // ✅ เช็คจำนวนผู้เล่นใน map ปัจจุบัน
        int playerCountInCurrentMap = CountPlayersInMap(currentMap);
        Log($"👥 Players in current map: {playerCountInCurrentMap}");

        // ✅ เทเลพอร์ตผู้เล่น
        yield return StartCoroutine(MovePlayerToMap(player, currentMap, targetMap));

        // ✅ ปิด map เก่าถ้าไม่มีผู้เล่นเหลืออยู่
        if (playerCountInCurrentMap <= 1) // ถ้ามีแค่คนเดียวหรือน้อยกว่า
        {
            yield return StartCoroutine(DeactivateMapSafely(currentMap));
        }
        else
        {
            Log($"🔵 Keeping map active - {playerCountInCurrentMap} players remaining");
        }

        // ✅ เปิด map ใหม่
        ActivateMap(targetMap);

        isTeleporting.Value = false;
        Log("✅ Teleport process completed");
    }

    private System.Collections.IEnumerator MovePlayerToMap(Player player, GameObject currentMap, GameObject targetMap)
    {
        // ✅ อัพเดท tracking
        RemovePlayerFromMap(player, currentMap);
        AddPlayerToMap(player, targetMap);

        // ✅ รอ 1 frame เพื่อให้ network อัพเดท
        yield return null;

        Log($"📍 Moved {player.Name} from {currentMap.name} to {targetMap.name}");
    }

    private System.Collections.IEnumerator DeactivateMapSafely(GameObject map)
    {
        if (map == null) yield break;

        // ✅ เช็คอีกครั้งก่อนปิด (ป้องกัน race condition)
        int finalPlayerCount = CountPlayersInMap(map);
        if (finalPlayerCount > 0)
        {
            Log($"🚫 Cancelled deactivation - {finalPlayerCount} players still in {map.name}");
            yield break;
        }

        // ✅ ปิด map
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

    // ✅ การจัดการผู้เล่นใน map
    public void AddPlayerToMap(Player player, GameObject map)
    {
        if (player == null || map == null) return;

        if (!playersInMap.ContainsKey(map))
        {
            playersInMap[map] = new List<Player>();
        }

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

    // ✅ เรียกเมื่อผู้เล่นสปawn หรือเชื่อมต่อ
    public void RegisterPlayerToMap(Player player, GameObject initialMap)
    {
        if (IsServer && player != null && initialMap != null)
        {
            AddPlayerToMap(player, initialMap);
        }
    }

    // ✅ เรียกเมื่อผู้เล่นตัดการเชื่อมต่อ
    public void UnregisterPlayer(Player player)
    {
        if (IsServer && player != null)
        {
            // ลบผู้เล่นจากทุก map
            foreach (var map in playersInMap.Keys)
            {
                RemovePlayerFromMap(player, map);
            }
            Log($"👋 Unregistered player: {player.Name}");
        }
    }

    private void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[TeleportManager] {message}");
        }
    }

    // ✅ สำหรับ debug ใน editor
    private void OnGUI()
    {
        if (debugMode && IsServer)
        {
            GUILayout.BeginArea(new Rect(10, 100, 300, 400));
            GUILayout.Label("🗺️ TELEPORT MANAGER DEBUG");
            GUILayout.Label($"IsTeleporting: {isTeleporting.Value}");

            foreach (var map in playersInMap.Keys)
            {
                GUILayout.Label($"{map.name}: {playersInMap[map].Count} players");
            }

            GUILayout.EndArea();
        }
    }
}
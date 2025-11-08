using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System.Collections;
using System.Linq;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TMP_InputField joinInputField;

    public GameObject UiStart;
    public GameObject UiJoin;

    private string savedJoinCode = "";
    private bool isReconnecting = false;

    async void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnect;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFail;

        if (!await HasInternetConnection())
        {
            NetworkManager.Singleton.StartHost();
            UiStart.SetActive(false);
            joinCodeText.text = "Offline Mode";
            return;
        }

        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            NetworkManager.Singleton.StartHost();
            UiStart.SetActive(false);
            joinCodeText.text = "Offline Mode";
        }
    }

    private async Task<bool> HasInternetConnection()
    {
        try
        {
            using var web = new System.Net.Http.HttpClient();
            web.Timeout = System.TimeSpan.FromSeconds(2);
            var result = await web.GetAsync("https://www.google.com");
            return result.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async void StartRelay()
    {
        savedJoinCode = await StartHostWithRelay();
        if (!string.IsNullOrEmpty(savedJoinCode))
        {
            joinCodeText.text = "Join Code: " + savedJoinCode;
            UiStart.SetActive(false);
        }
    }

    public async void JoinRelay()
    {
        savedJoinCode = joinInputField.text;
        bool success = await StartClientWithRelay(savedJoinCode);

        if (success)
        {
            joinCodeText.text = "Joined!";
            UiJoin.SetActive(false);
        }
        else joinCodeText.text = "Join Failed!";
    }

    private async Task<string> StartHostWithRelay(int maxConnections = 4)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }
        catch { return null; }
    }

    private async Task<bool> StartClientWithRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            return NetworkManager.Singleton.StartClient();
        }
        catch { return false; }
    }

    private async void OnDisconnect(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsClient
            || clientId != NetworkManager.Singleton.LocalClientId)
            return;

        await TryReconnect();
    }

    private async void OnTransportFail()
    {
        await TryReconnect();
    }

    private async Task TryReconnect()
    {
        if (isReconnecting) return;
        isReconnecting = true;

        for (int i = 0; i < 5; i++)
        {
            if (!await HasInternetConnection())
            {
                await Task.Delay(2000);
                continue;
            }

            if (await StartClientWithRelay(savedJoinCode))
            {
                Debug.Log("✅ Reconnect Success");

                StartCoroutine(RestorePlayerPositionAfterSpawn());

                isReconnecting = false;
                return;
            }

            await Task.Delay(1500);
        }

        isReconnecting = false;
        ReturnToLobby();
    }

    private IEnumerator RestorePlayerPositionAfterSpawn()
    {
        Player player = null;

        while (player == null)
        {
            player = FindObjectsByType<Player>(FindObjectsSortMode.None)
                .FirstOrDefault(p => p.IsOwner);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // ✅ ไม่ต้อง set ตรงนี้ เพราะ Identity.cs auto restore ให้แล้ว
        Debug.Log("✅ Player Spawn detected. Position will restore automatically.");
    }

    private void ReturnToLobby()
    {
        GoToLobbyScene();
    }
    public void GoToLobbyScene()
    {
        if (SceneTransitionHandler.Instance != null)
        {
            SceneTransitionHandler.Instance.GoToLobbyScene();
        }
        else
        {
            // สำรองในกรณีที่ Handler ไม่พร้อม
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
    public void LoadGameScene(string sceneName)
    {
        // 🚨 4. ใช้ NetworkSceneManager.LoadScene() เพื่อสั่งโหลด Scene ใหม่
        // โค้ดนี้ต้องถูกเรียกโดย Host/Server เท่านั้น!
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Only the Host/Server can initiate scene loading via Netcode.");
        }
    }
}
